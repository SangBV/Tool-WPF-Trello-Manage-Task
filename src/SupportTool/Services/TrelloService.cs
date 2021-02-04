using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SupportTool.Model.Trello;
using SupportTool.Model.Trello.Entities;
using SupportTool.Utilities.Helper;
using SupportTool.ViewModel.Trello;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using TRELLO_CONSTANT = SupportTool.Constant.Constant.TrelloConstant;

namespace SupportTool.Services
{
    public class TrelloService
    {
        #region Declaration

        static public List<ListName> trelloList = new List<ListName>();
        static public ListCardViewModel listCardViewModel = new ListCardViewModel();
        static public List<Label> trelloLabel = new List<Label>();
        static public List<CustomField> trelloCustomField = new List<CustomField>();
        static public URLParamModel paramModel = new URLParamModel();

        private CommonHelper _commonHelper;
        public string JSON_DATA_EXPORT_PATH;
        private MappingService _mapper;

        public DateTimeHelper dateTimeHelper;

        #endregion

        #region Constructor

        public TrelloService()
        {
            dateTimeHelper = new DateTimeHelper();
            _commonHelper = new CommonHelper();
            _mapper = new MappingService();
            _mapper.InitializeMapper();

            //Common
            JSON_DATA_EXPORT_PATH = _commonHelper.GetRootPath(@"assets\json-data");

            paramModel.TrelloBoardId = _commonHelper.GetDataFromAppKey("TrelloBoardId");

            string trelloKey = _commonHelper.GetDataFromAppKey("TrelloKey");
            string trelloToken = _commonHelper.GetDataFromAppKey("TrelloToken");
            paramModel.TrelloKeyToken = $"key={trelloKey}&token={trelloToken}";
        }

        #endregion

        #region Todo Task Management- Features

        public void CloneData()
        {
            CorrectCardInfo();
            CloneAllTrelloData();
        }

        public void AddTask(CardAddNewViewModel model)
        {
            var cardTemplate = new TrelloCardTemplate();
            var labelNames = new List<string>();
            var customFields = new List<CustomFieldTemplate>();

            switch (model.Type)
            {
                case TRELLO_CONSTANT.SideType.Default:
                    customFields = new List<CustomFieldTemplate>()
                    {
                        new CustomFieldTemplate
                        {
                            Name = TRELLO_CONSTANT.CustomField.Assignee,
                            Type = TRELLO_CONSTANT.CustomFieldType.Text,
                            ValueText = new ValueText { Text = "SangBV" }
                        }
                    };

                    if (model.DuedateAt.HasValue)
                    {
                        customFields.Add(
                               new CustomFieldTemplate
                               {
                                   Name = TRELLO_CONSTANT.CustomField.DuedateAt,
                                   Type = TRELLO_CONSTANT.CustomFieldType.Date,
                                   ValueDate = new ValueDate { Date = dateTimeHelper.ConvertDateToUTC(model.DuedateAt.Value) }
                               });
                    }

                    cardTemplate.IdList = GetListDetailByNames(TRELLO_CONSTANT.ListName.NextTodo);
                    cardTemplate.CustomField = GetListCustomFieldDetail(customFields);
                    break;
            }

            if (!string.IsNullOrEmpty(cardTemplate.IdList))
            {
                var maxPosInList = listCardViewModel.Cards.Where(x => x.ListName == TRELLO_CONSTANT.ListName.NextTodo).OrderByDescending(x => x.Pos).FirstOrDefault()?.Pos;

                var card = new Card
                {
                    Name = model.Name,
                    Pos = maxPosInList != null && maxPosInList.HasValue ? maxPosInList.Value + 1 : 0,
                    IdBoard = paramModel.TrelloBoardId,
                    IdList = cardTemplate.IdList,
                    IdLabels = cardTemplate.IdLabels
                };

                string URL = GetTrelloAPIByType(TRELLO_CONSTANT.TrelloAPIType.CardCreateNew);

                var response = CallAPI(TRELLO_CONSTANT.APIMethod.POST, URL, card);

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = response.Content.ReadAsStringAsync().Result;

                    var cardAdded = JsonConvert.DeserializeObject<Card>(jsonData);

                    //Update custom field
                    foreach (var field in cardTemplate.CustomField)
                    {
                        CustomFieldUpdateInCard(cardAdded.Id, field);
                    }
                }
            }
        }

        #endregion

        #region Functional

        public void DetectAndClarifyData()
        {
            CloneData();

            var customFieldEffortInDay = new CustomFieldTemplate
            {
                Name = TRELLO_CONSTANT.CustomField.EffortInDay,
                Type = TRELLO_CONSTANT.CustomFieldType.Number
            };
            customFieldEffortInDay = GetCustomFieldDetail(customFieldEffortInDay);
            var customFieldEffortInDayId = customFieldEffortInDay.Id;

            var listNamedDoneId = trelloList.Where(x => x.Name == TRELLO_CONSTANT.ListName.Completed).FirstOrDefault()?.Id;
            var listNamedArchivedId = trelloList.Where(x => x.Name == TRELLO_CONSTANT.ListName.Closed).FirstOrDefault()?.Id;
            if (!string.IsNullOrEmpty(listNamedDoneId) && !string.IsNullOrEmpty(listNamedArchivedId))
            {
                //get all card exists in List named "Completed"
                var listCartDone = listCardViewModel.Cards.Where(x => x.IdList == listNamedDoneId).ToList();
                int totalCardDone = listCartDone != null ? listCartDone.Count() : 0;
                
                var logData = ReadAndConvertJsonfileToObject<List<TrackingTaskEffort>>($@"{JSON_DATA_EXPORT_PATH}\TrackingTaskEffort.json") ?? new List<TrackingTaskEffort>();
                if(logData == null || logData.Count() == 0)
                {
                    WriteConvertObjectToJson($@"{JSON_DATA_EXPORT_PATH}\TrackingTaskEffort.json", logData);
                }

                Console.WriteLine("Has found ({0}) card exist in Done List", totalCardDone);
                if (totalCardDone > 0)
                {
                    #region Log Task Effort In Day

                    if (!string.IsNullOrEmpty(customFieldEffortInDayId))
                    {
                        var isExistLogDataToday = logData.Any(x => x.Date.Date == DateTime.Now.Date);

                        if (isExistLogDataToday)
                        {
                            //update log data
                            foreach (var item in logData)
                            {
                                if (item.Date.Date == DateTime.Now.Date)
                                {
                                    var cardNotInsertToLogData = listCartDone.Where(x => !item.IdCards.Contains(x.Id));

                                    if (cardNotInsertToLogData != null && cardNotInsertToLogData.Count() > 0)
                                    {
                                        float totalEffortInday = 0;
                                        foreach (var card in cardNotInsertToLogData)
                                        {
                                            foreach (var ctf in card.CustomFieldItems)
                                            {
                                                if (ctf.IdCustomField == customFieldEffortInDayId)
                                                {
                                                    totalEffortInday += !string.IsNullOrEmpty(ctf.Value.Number) ? float.Parse(ctf.Value.Number) : 0;
                                                }
                                            }
                                        }

                                        item.LastModifiedAt = DateTime.Now;
                                        item.IdCards.AddRange(cardNotInsertToLogData.Select(x => x.Id).ToList());
                                        item.TotalEffort += totalEffortInday;
                                        item.TotalTask = item.IdCards != null ? item.IdCards.Where(x => !string.IsNullOrEmpty(x)).Count() : 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var cardEffortHaveValue = listCartDone.Where(x => x.CustomFieldItems.Select(y => y.IdCustomField).Contains(customFieldEffortInDayId));
                            float totalEffortInday = 0;
                            foreach (var card in cardEffortHaveValue)
                            {
                                foreach (var ctf in card.CustomFieldItems)
                                {
                                    if (ctf.IdCustomField == customFieldEffortInDayId)
                                    {
                                        totalEffortInday += !string.IsNullOrEmpty(ctf.Value.Number) ? float.Parse(ctf.Value.Number) : 0;
                                    }
                                }
                            }
                            //add new log data
                            logData.Add(new TrackingTaskEffort
                            {
                                Date = DateTime.Now,
                                LastModifiedAt = DateTime.Now,
                                TotalEffort = totalEffortInday,
                                TotalTask = totalCardDone,
                                IdCards = listCartDone.Select(x => x.Id).ToList()
                            });
                        }

                        //Override log data
                        WriteConvertObjectToJson($@"{JSON_DATA_EXPORT_PATH}\TrackingTaskEffort.json", logData);
                    }

                    #endregion

                    #region Move card from Completed to Closed

                    foreach (var card in listCartDone)
                    {
                        paramModel.TrelloCardId = card.Id;
                        paramModel.TrelloIdList = listNamedArchivedId;

                        string URL = GetTrelloAPIByType(TRELLO_CONSTANT.TrelloAPIType.CardMoveToOtherList);

                        var response = CallAPI(TRELLO_CONSTANT.APIMethod.PUT, URL);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(">>Moved card ({0}) to Archived list successfully.", card.Name);
                        }
                        else
                        {
                            Console.WriteLine(">>Failed to move card ({0}), {1}: ({2})", card.Name, (int)response.StatusCode, response.ReasonPhrase);
                        }
                    }

                    #endregion
                }
            }

            CloneData();
        }

        public void CloneAllTrelloData()
        {
            trelloList = new List<ListName>();
            listCardViewModel = new ListCardViewModel();
            trelloLabel = new List<Label>();
            trelloCustomField = new List<CustomField>();

            GetTrelloDataByType(TRELLO_CONSTANT.TrelloAPIType.ListGetAll);
            GetTrelloDataByType(TRELLO_CONSTANT.TrelloAPIType.CardGetAll);
            GetTrelloDataByType(TRELLO_CONSTANT.TrelloAPIType.LabelGetAll);
            GetTrelloDataByType(TRELLO_CONSTANT.TrelloAPIType.CustomFieldGetAll);
            GetCustomCardData();
        }

        private void CorrectCardInfo()
        {
            //To fetch data at first
            CloneAllTrelloData();

            IncludeCardNumber();
            UpdateCustomfieldPercentage();

            //Update finish date at customfield in Completed list
            var customFieldUpdate = new CustomFieldTemplate
            {
                Name = TRELLO_CONSTANT.CustomField.FinishDateAt,
                Type = TRELLO_CONSTANT.CustomFieldType.Date,
                ValueDate = new ValueDate { Date = dateTimeHelper.ConvertDateToUTC(DateTime.Today) }
            };

            customFieldUpdate = GetCustomFieldDetail(customFieldUpdate);
            UpdateCustomfieldInList(TRELLO_CONSTANT.ListName.Completed, isForce: false, customFieldUpdate);
        }

        public void GetCustomCardData()
        {
            var totalCard = listCardViewModel != null && listCardViewModel.Cards != null ? listCardViewModel.Cards.Count() : 0;
            if (totalCard > 0)
            {
                foreach (var card in listCardViewModel.Cards)
                {
                    card.ListName = trelloList.FirstOrDefault(x => x.Id == card.IdList)?.Name;

                    if (card.IdLabels != null && card.IdLabels.Count() > 0)
                    {
                        card.ListLabelTagged = new List<string>();
                        foreach (var label in card.IdLabels)
                        {
                            var labelFound = trelloLabel.FirstOrDefault(x => x.Id == label)?.Name;
                            if (!string.IsNullOrEmpty(labelFound))
                            {
                                card.ListLabelTagged.Add(labelFound);
                            }
                        }
                    }

                    if(card.CustomFieldItems != null && card.CustomFieldItems.Count() > 0)
                    {
                        foreach (var field in card.CustomFieldItems)
                        {
                            var customFieldFound = trelloCustomField.FirstOrDefault(x => x.Id == field.IdCustomField);
                            if (customFieldFound != null)
                            {
                                field.Name = customFieldFound.Name;
                                field.Type = customFieldFound.Type;
                            }
                        }
                    }
                }
            }

            //Override log data
            WriteConvertObjectToJson($@"{JSON_DATA_EXPORT_PATH}\TrelloCardGetAll.json", listCardViewModel);
        }

        public void UpdateCustomfieldPercentage()
        {
            var customField = new CustomFieldTemplate()
            {
                Name = TRELLO_CONSTANT.CustomField.Percent,
                Type = TRELLO_CONSTANT.CustomFieldType.Number
            };
            var customFieldPercent = GetCustomFieldDetail(customField);

            //update cards have checklist
            var listCard = listCardViewModel.Cards.Where(x => x.Badges.CheckItems > 0).ToList();
            int totalCardFound = listCard != null ? listCard.Count() : 0;

            if (totalCardFound > 0)
            {
                foreach (var card in listCard)
                {
                    var currentPercent = card.CustomFieldItems.FirstOrDefault(s => s.Name == TRELLO_CONSTANT.CustomField.Percent && s.Type == TRELLO_CONSTANT.CustomFieldType.Number)?.Value.Number;

                    if (card.Badges.CheckItems == card.Badges.CheckItemsChecked 
                        && (!string.IsNullOrEmpty(currentPercent) && currentPercent == "100"))
                    {
                        continue;
                    }
                    else
                    {
                        var customFieldUpdate = customFieldPercent;
                        if (customFieldUpdate != null)
                        {
                            customFieldUpdate.ValueNumber = new ValueNumber
                            {
                                Number = card.Badges.CheckItems > 0 ? Math.Round(((double)card.Badges.CheckItemsChecked / (double)card.Badges.CheckItems) * 100).ToString() : "0"
                            };

                            CustomFieldUpdateInCard(card.Id, customFieldUpdate);
                        }
                    }
                }
            }

            //update cards are in "Completed" list
            var listCardInCompleted = listCardViewModel.Cards.Where(x => x.Badges.CheckItems <= 0 && x.ListName == TRELLO_CONSTANT.ListName.Completed).ToList();
            totalCardFound = listCardInCompleted != null ? listCardInCompleted.Count() : 0;
            if (totalCardFound > 0)
            {
                foreach (var card in listCardInCompleted)
                {
                    var customFieldUpdate = customFieldPercent;
                    if (customFieldUpdate != null)
                    {
                        customFieldUpdate.ValueNumber = new ValueNumber
                        {
                            Number = "100"
                        };

                        CustomFieldUpdateInCard(card.Id, customFieldUpdate);
                    }
                }
            }
        }

        public void GetTrelloDataByType(string type)
        {
            string URL = GetTrelloAPIByType(type);

            var response = CallAPI(TRELLO_CONSTANT.APIMethod.GET, URL);

            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                var jsonData = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
                System.IO.File.WriteAllText($@"{JSON_DATA_EXPORT_PATH}\Trello{type}.json", jsonData);

                ConvertJsonToObject(jsonData, type);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
        }

        public string GetTrelloAPIByType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return string.Empty;
            }
            else
            {
                var apiResult = "";
                var apiPointTop = "";

                string trelloRootURL = _commonHelper.GetDataFromAppKey("TrelloRootURL");

                switch (type)
                {
                    case TRELLO_CONSTANT.TrelloAPIType.ListGetAll:
                        apiPointTop = _commonHelper.GetDataFromAppKey("TrelloGetListAPI");
                        apiResult = trelloRootURL + GenerateAPI(apiPointTop);
                        break;
                    case TRELLO_CONSTANT.TrelloAPIType.CardGetAll:
                        apiPointTop = _commonHelper.GetDataFromAppKey("TrelloGetCardAPI");
                        apiResult = trelloRootURL + GenerateAPI(apiPointTop);
                        break;
                    case TRELLO_CONSTANT.TrelloAPIType.LabelGetAll:
                        apiPointTop = _commonHelper.GetDataFromAppKey("TrelloGetLabelAPI");
                        apiResult = trelloRootURL + GenerateAPI(apiPointTop);
                        break;
                    case TRELLO_CONSTANT.TrelloAPIType.CardMoveToOtherList:
                        apiPointTop = _commonHelper.GetDataFromAppKey("TrelloMoveCardToOtherList");
                        apiResult = trelloRootURL + GenerateAPI(apiPointTop);
                        break;
                    case TRELLO_CONSTANT.TrelloAPIType.CardUpdate:
                        apiPointTop = _commonHelper.GetDataFromAppKey("TrelloUpdateCard");
                        apiResult = trelloRootURL + GenerateAPI(apiPointTop);
                        break;
                    case TRELLO_CONSTANT.TrelloAPIType.CardCreateNew:
                        apiPointTop = _commonHelper.GetDataFromAppKey("TrelloCardCreateNew");
                        apiResult = trelloRootURL + GenerateAPI(apiPointTop);
                        break;
                    case TRELLO_CONSTANT.TrelloAPIType.CustomfieldUpdateInCard:
                        apiPointTop = _commonHelper.GetDataFromAppKey("TrelloUpdateCustomField");
                        apiResult = trelloRootURL + GenerateAPI(apiPointTop);
                        break;
                    case TRELLO_CONSTANT.TrelloAPIType.CustomFieldGetAll:
                        apiPointTop = _commonHelper.GetDataFromAppKey("TrelloCustomFieldGetAll");
                        apiResult = trelloRootURL + GenerateAPI(apiPointTop);
                        break;
                    default:
                        break;
                }

                return apiResult;
            }
        }
        public string GenerateAPI(string value)
        {
            return value
                .Replace("{boardId}", paramModel.TrelloBoardId)
                .Replace("{keyToken}", paramModel.TrelloKeyToken)
                .Replace("{cardId}", paramModel.TrelloCardId)
                .Replace("{idList}", paramModel.TrelloIdList)
                .Replace("{customFieldId}", paramModel.TrelloCustomfieldId);
        }

        #endregion

        #region Trello 

        #region Label

        public List<string> GetListLabelIdByName(List<string> names)
        {
            var result = new List<string>();
            if(names == null || names.Count() == 0)
            {
                return result;
            }

            result = trelloLabel.Where(x => names.Contains(x.Name)).Select(x => x.Id).ToList();

            return result;
        }

        #endregion

        #region Custom Field

        public List<CustomFieldTemplate> GetListCustomFieldDetail(List<CustomFieldTemplate> customeField, bool isExistId = false)
        {
            if (customeField == null || customeField.Count() == 0)
            {
                return customeField;
            }
            var result = new List<CustomFieldTemplate>();
            foreach (var field in customeField)
            {
                var customFieldFound = trelloCustomField.Where(x => (isExistId && x.Id == field.Id)  
                                                            || (!isExistId && x.Name == field.Name && x.Type == field.Type)).FirstOrDefault();
                if(customFieldFound != null)
                {
                    var customFieldAdd = field;
                    customFieldAdd.Id = customFieldFound.Id;
                    customFieldAdd.Type = customFieldFound.Type;
                    customFieldAdd.Name = customFieldFound.Name;

                    result.Add(customFieldAdd);
                }
            }
            return result;
        }

        public CustomFieldTemplate GetCustomFieldDetail(CustomFieldTemplate customeField)
        {
            if (customeField == null)
            {
                return customeField;
            }
            var customFieldFoundId = trelloCustomField.Where(x => x.Name == customeField.Name && x.Type == customeField.Type).Select(x => x.Id).FirstOrDefault();
            customeField.Id = customFieldFoundId;

            return customeField;
        }

        public void UpdateCustomfieldInList(string listName, bool isForce, CustomFieldTemplate customField)
        {
            var listCard = listCardViewModel.Cards.Where(x => x.ListName == listName
                && (isForce
                    || (!isForce && !x.CustomFieldItems.Any(c => c.IdCustomField == customField.Id))
                )).ToList();

            if (listCard != null && listCard.Count() > 0)
            {
                foreach (var card in listCard)
                {
                    if (customField != null)
                    {
                        CustomFieldUpdateInCard(card.Id, customField);
                    }
                }
            }
        }
        public void CustomFieldUpdateInCard(string cardId, CustomFieldTemplate customField)
        {
            paramModel.TrelloCardId = cardId;
            paramModel.TrelloCustomfieldId = customField.Id;

            if (string.IsNullOrEmpty(paramModel.TrelloCardId) || string.IsNullOrEmpty(paramModel.TrelloCustomfieldId))
            {
                return;
            }

            var fieldDate = new TrelloCustomFieldUpdate<ValueDate>() { Value = customField.ValueDate };
            var fieldText = new TrelloCustomFieldUpdate<ValueText>() { Value = customField.ValueText };
            var fieldNumber = new TrelloCustomFieldUpdate<ValueNumber>() { Value = customField.ValueNumber };

            string URL = GetTrelloAPIByType(TRELLO_CONSTANT.TrelloAPIType.CustomfieldUpdateInCard);

            var response = new HttpResponseMessage();

            switch (customField.Type)
            {
                case TRELLO_CONSTANT.CustomFieldType.Date:
                    response = CallAPI(TRELLO_CONSTANT.APIMethod.PUT, URL, fieldDate);
                    break;
                case TRELLO_CONSTANT.CustomFieldType.Text:
                    response = CallAPI(TRELLO_CONSTANT.APIMethod.PUT, URL, fieldText);
                    break;
                case TRELLO_CONSTANT.CustomFieldType.Number:
                    response = CallAPI(TRELLO_CONSTANT.APIMethod.PUT, URL, fieldNumber);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region List

        public string GetListDetailByNames(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var idList = trelloList.Where(x => x.Name == name).Select(x => x.Id).FirstOrDefault();

            return idList;
        }

        #endregion

        #region Card

        public void TagLabelIntoAllCardList(string labelName, string listName)
        {
            Console.WriteLine("\n--------------------------------------------------------------------");
            var labelFound = trelloLabel.Where(x => x.Name == labelName).FirstOrDefault();

            if (labelFound == null)
            {
                Console.WriteLine($"{labelName} is not found. Can not add it into card");
            }
            else
            {
                var cardInList = listCardViewModel.Cards.Where(x => x.ListName == listName).ToList();
                var cardNotTaggedLabelName = cardInList.Where(x => x.ListLabelTagged == null || !x.ListLabelTagged.Contains(labelName));

                var totalCardFound = cardNotTaggedLabelName != null ? cardNotTaggedLabelName.Count() : 0;
                Console.WriteLine($"Has found ({totalCardFound}) card need to be added label {labelName}");
                //Card need to add label name
                foreach (var card in cardNotTaggedLabelName)
                {
                    if (card.IdLabels == null || card.IdLabels.Count() == 0)
                    {
                        card.IdLabels = new List<string>();
                        card.IdLabels.Add(labelFound.Id);
                    }
                    else
                    {
                        card.IdLabels.Add(labelFound.Id);
                    }

                    //call API add label to card
                    try
                    {
                        paramModel.TrelloCardId = card.Id;
                        string URL = GetTrelloAPIByType(TRELLO_CONSTANT.TrelloAPIType.CardUpdate);

                        var response = CallAPI(TRELLO_CONSTANT.APIMethod.PUT, URL, card);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($">>Added (1) label {labelName} into Card {card.Name}");
                        }
                        else
                        {
                            Console.WriteLine(">>Failed to add label into card ({0}), {1}: ({2})", card.Name, (int)response.StatusCode, response.ReasonPhrase);
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
            Console.WriteLine("--------------------------------------------------------------------");
        }

        public void RemoveLabelForAllCardInList(string labelName, string listName)
        {
            Console.WriteLine("\n--------------------------------------------------------------------");
            var labelFound = trelloLabel.Where(x => x.Name == labelName).FirstOrDefault();

            if (labelFound == null)
            {
                Console.WriteLine($"{labelName} is not found. Can not add it into card");
            }
            else
            {
                var cardInList = listCardViewModel.Cards.Where(x => x.ListName == listName).ToList();
                var cardTaggedLabelName = cardInList.Where(x => x.ListLabelTagged.Contains(labelName));

                var totalCardFound = cardTaggedLabelName != null ? cardTaggedLabelName.Count() : 0;
                Console.WriteLine($"Has found ({totalCardFound}) card need to remove label {labelName} in list {listName}");
                //Card need to add label name
                foreach (var card in cardTaggedLabelName)
                {
                    if (card.IdLabels != null || card.IdLabels.Count() > 0)
                    {
                        //call API add label to card
                        try
                        {
                            paramModel.TrelloCardId = card.Id;

                            card.IdLabels = card.IdLabels.Where(x => !x.Contains(labelFound.Id)).ToList();

                            string URL = GetTrelloAPIByType(TRELLO_CONSTANT.TrelloAPIType.CardUpdate);

                            var response = CallAPI(TRELLO_CONSTANT.APIMethod.PUT, URL, card);

                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($">>Removed (1) label {labelName} out of Card {card.Name}");
                            }
                            else
                            {
                                Console.WriteLine(">>Failed to remove label out of card ({0}), {1}: ({2})", card.Name, (int)response.StatusCode, response.ReasonPhrase);
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                }
            }
            Console.WriteLine("--------------------------------------------------------------------");
        }

        public void IncludeCardNumber()
        {
            Console.WriteLine("\n--------------------------------------------------------------------");
            //get all card doesn't exist number id
            var listCart = listCardViewModel.Cards.Where(x => !x.Name.Contains("#No.")).ToList();
            int totalCardFound = listCart != null ? listCart.Count() : 0;
            Console.WriteLine("Has found ({0}) card need to be included the number id", totalCardFound);
            if (totalCardFound > 0)
            {
                try
                {
                    foreach (var card in listCart)
                    {
                        paramModel.TrelloCardId = card.Id;

                        var cardUpdate = card;
                        cardUpdate.Name = GenerateCardNumberDisplay(card.Name, card.IdShort);

                        string URL = GetTrelloAPIByType(TRELLO_CONSTANT.TrelloAPIType.CardUpdate);

                        var response = CallAPI(TRELLO_CONSTANT.APIMethod.PUT, URL, cardUpdate);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(">>Update card ({0}) successfully.", card.Name);
                        }
                        else
                        {
                            Console.WriteLine(">>Failed to update card ({0}), {1}: ({2})", card.Name, (int)response.StatusCode, response.ReasonPhrase);
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            Console.WriteLine("--------------------------------------------------------------------");
        }
        static string GenerateCardNumberDisplay(string cardName, int idShort)
        {
            if (!string.IsNullOrEmpty(cardName) && !cardName.Contains("#No."))
            {
                var result = $"#No.{idShort} | {cardName}";
                return result;
            }
            return cardName;
        }

        #endregion

        #endregion

        #region Helper Method

        static HttpResponseMessage CallAPI(string method, string url, object json = null)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            if (!string.IsNullOrEmpty(url))
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(url);

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                switch (method)
                {
                    case TRELLO_CONSTANT.APIMethod.GET:
                        response = client.GetAsync(url).Result; // Blocking call! Program will wait here until a response is received or a timeout occurs.
                        break;
                    case TRELLO_CONSTANT.APIMethod.PUT:
                        if (json != null)
                        {
                            var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                            var jsonConvert = JsonConvert.SerializeObject(json, camelSettings);
                            var content = new StringContent(jsonConvert, System.Text.Encoding.UTF8, "application/json");
                            response = client.PutAsync(url, content).Result; // Blocking call! Program will wait here until a response is received or a timeout occurs.
                        }
                        else
                        {
                            response = client.PutAsync(url, null).Result; // Blocking call! Program will wait here until a response is received or a timeout occurs.
                        }
                        break;
                    case TRELLO_CONSTANT.APIMethod.POST:
                        if (json != null)
                        {
                            var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                            var jsonConvert = JsonConvert.SerializeObject(json, camelSettings);
                            var content = new StringContent(jsonConvert, System.Text.Encoding.UTF8, "application/json");
                            response = client.PostAsync(url, content).Result; // Blocking call! Program will wait here until a response is received or a timeout occurs.
                        }
                        else
                        {
                            response = client.PostAsync(url, null).Result; // Blocking call! Program will wait here until a response is received or a timeout occurs.
                        }
                        break;
                    default:
                        break;
                }

                //Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
                client.Dispose();

            }
            return response;
        }

        static T ReadAndConvertJsonfileToObject<T>(string path)
        {
            var data = "";

            if (File.Exists(path))
            {
                data = System.IO.File.ReadAllText($@"{path}");
            }

            var result = JsonConvert.DeserializeObject<T>(data);
            return result;
        }

        static void WriteConvertObjectToJson(string path, object data)
        {
            var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            System.IO.File.WriteAllText($@"{path}", JsonConvert.SerializeObject(data, camelSettings));
        }

        public void ConvertJsonToObject(string json, string type)
        {
            switch (type)
            {
                case TRELLO_CONSTANT.TrelloAPIType.ListGetAll:
                    trelloList = JsonConvert.DeserializeObject<List<ListName>>(json);
                    break;
                case TRELLO_CONSTANT.TrelloAPIType.CardGetAll:
                    var listCard = JsonConvert.DeserializeObject<ListCard>(json);
                    listCardViewModel = Mapper.Map<ListCardViewModel>(listCard);
                    break;
                case TRELLO_CONSTANT.TrelloAPIType.LabelGetAll:
                    trelloLabel = JsonConvert.DeserializeObject<List<Label>>(json);
                    break;
                case TRELLO_CONSTANT.TrelloAPIType.CustomFieldGetAll:
                    trelloCustomField = JsonConvert.DeserializeObject<List<CustomField>>(json);
                    break;
                default:
                    break;
            }

        }

        #endregion
    }
}
