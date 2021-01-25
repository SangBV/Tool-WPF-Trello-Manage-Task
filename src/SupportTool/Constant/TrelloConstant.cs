namespace SupportTool.Constant
{
    public partial class Constant
    {
        public struct TrelloConstant
        {
            public struct SideType
            {
                public const string Default = "Default";
            }

            public struct TrelloAPIType
            {
                public const string ListGetAll = "ListGetAll";
                public const string LabelGetAll = "LabelGetAll";
                public const string CustomFieldGetAll = "CustomFieldGetAll";

                public const string CardGetAll = "CardGetAll";
                public const string CardMoveToOtherList = "CardMoveToOtherList";
                public const string CardUpdate = "CardUpdate";
                public const string CardCreateNew = "CardCreateNew";

                public const string CustomfieldUpdateInCard = "CustomfieldUpdateInCard";
            }

            public struct ListName
            {
                public const string Todo = "Todo";
                public const string NextTodo = "Next Todo";
                public const string InProgress = "In-Progress";
                public const string Pending = "Pending";
                public const string Blocked = "Blocked";
                public const string Completed = "Completed";
                public const string Closed = "Closed";
            }

            public struct LabelName
            {
                public const string Follow = "Follow";
                public const string Checked = "Checked";
                public const string Completed = "Completed";
                public const string InProgress = "In-Progress";
                public const string Stuck = "Stuck";
            }

            public struct CustomField
            {
                public const string EffortInDay = "Effort In day (hour)";
                public const string Percent = "Percent";
                public const string DuedateAt = "Duedate At";
                public const string Assignee = "Assignee";
                public const string FinishDateAt = "Finish Date At";
            }

            public struct CustomFieldType
            {
                public const string Date = "date";
                public const string Text = "text";
                public const string Number = "number";
            }
            public struct APIMethod
            {
                public const string GET = "GET";
                public const string PUT = "PUT";
                public const string POST = "POST";
            }
        }
    }
}
