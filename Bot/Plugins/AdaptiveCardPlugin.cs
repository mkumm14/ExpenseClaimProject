// AdaptiveCardPlugin.cs
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace ExpenseClaimProject.Bot.Plugins
{
    public class AdaptiveCardPlugin
    {



        [KernelFunction("GenerateEditCardCardType")]
        [Description("Generates an adaptive card for editing an expense claim when the payment method given is card type.")]
        public string GenerateEditCardCardType(string merchantName, string transactionDate, string total, string currency, string category, string paymentMethod, string fourDigits, string notes = "")
        {

            var card = new
            {
                type = "AdaptiveCard",
                body = new List<object>
                {
                    new
                    {
                        type = "TextBlock",
                        text = "Expense Claim Form",
                        weight = "Bolder",
                        size = "Medium"
                    },
                    new
                    {
                        type = "Input.Text",
                        id = "merchantName",
                        placeholder = "Merchant Name",
                        value = merchantName ?? "",
                        isRequired = true,
                        label = "Merchant Name"
                    },
                    new
                    {
                        type = "Input.Date",
                        id = "transactionDate",
                        placeholder = "Transaction Date",
                        value = transactionDate ?? "",
                        isRequired = true,
                        label = "Transaction Date"
                    },
                    new
                    {
                        type = "Input.Number",
                        id = "total",
                        placeholder = "Total Amount",
                        isRequired = true,
                        value = total ?? "",
                        label = "Total Amount"
                    },
                    new
                    {
                        type = "Input.Text",
                        id = "currency",
                        placeholder = "Currency (e.x. USD, INR etc.)",
                        value = currency ?? "",
                        isRequired = true,
                        label = "Currency",

                    },
                    new
                    {
                        type = "Input.ChoiceSet",
                        id = "expenseType",
                        style = "Compact",
                        isMultiSelect = false,
                        value = "",
                        isRequired = true,
                        label = "Expense Type",
                        choices = new List<object>
                        {
                            new { title = "Corporate", value = "Corporate" },
                            new { title = "Personal", value = "Personal" },
                        }
                    },
                    new
                    {
                        type = "Input.ChoiceSet",
                        id = "category",
                        style = "Compact",
                        isMultiSelect = false,
                        value = category ?? "",
                        isRequired = true,
                        label = "Category",
                        choices = new List<object>
                        {
                            new { title = "Travel", value = "Travel" },
                            new { title = "Meals", value = "Meals" },
                            new { title = "Supplies", value = "Supplies" },
                            new { title = "Other", value = "Other" }
                        }
                    },
                    new
                    {
                        type = "Input.ChoiceSet",
                        id = "paymentMethod",
                        style = "Compact",
                        isMultiSelect = false,
                        value = paymentMethod ?? "",
                        isRequired = true,
                        label = "Payment Method",
                        choices = new List<object>
                        {
                            new { title = "Credit Card", value = "Credit Card"},
                            new { title = "Debit Card", value = "Debit Card" },
                            new { title = "Cash", value = "Cash" },
                            new { title = "Other", value = "Other" }

                        }

                    },
                    new
                    {
                    type = "Input.Text",
                    id = "fourDigits",
                    isRequired = true,
                    placeholder = "Last 4 Digits of Card",
                    value = fourDigits ?? "",
                    label = "Last 4 Digits of Card"
                },
                     new
                    {
                        type = "Input.Text",
                        id = "notes",
                        placeholder = "Notes (optional)",
                        value = notes ?? "",
                        label = "Notes"
                    }

                    },
                actions = new List<object>
        {
            new
            {
                type = "Action.Submit",
                title = "Done with Edit",
                data = new { action = "submitValidationForm" }
            }
        }
            };



            Console.WriteLine(card);


            return JsonSerializer.Serialize(card);




        }



        [KernelFunction("GenerateEditCardNonCardType")]
        [Description("Generates an adaptive card for editing an expense claim when the payment method given is non-card type like cash.")]
        public string GenerateEditCardNonCardType(string merchantName, string transactionDate, string total, string currency, string category, string paymentMethod, string fourDigits, string notes = "")
        {

            var card = new
            {
                type = "AdaptiveCard",
                body = new List<object>
                {
                    new
                    {
                        type = "TextBlock",
                        text = "Expense Claim Form",
                        weight = "Bolder",
                        size = "Medium"
                    },
                    new
                    {
                        type = "Input.Text",
                        id = "merchantName",
                        placeholder = "Merchant Name",
                        value = merchantName ?? "",
                        isRequired = true,
                        label = "Merchant Name"
                    },
                    new
                    {
                        type = "Input.Date",
                        id = "transactionDate",
                        placeholder = "Transaction Date",
                        value = transactionDate ?? "",
                        isRequired = true,
                        label = "Transaction Date"
                    },
                    new
                    {
                        type = "Input.ChoiceSet",
                        id = "expenseType",
                        style = "Compact",
                        isMultiSelect = false,
                        value = "",
                        isRequired = true,
                        label = "Expense Type",
                        choices = new List<object>
                        {
                            new { title = "Corporate", value = "Corporate" },
                            new { title = "Personal", value = "Personal" },
                        }
                    },
                    new
                    {
                        type = "Input.Number",
                        id = "total",
                        placeholder = "Total Amount",
                        isRequired = true,
                        value = total ?? "",
                        label = "Total Amount"
                    },
                    new
                    {
                        type = "Input.Text",
                        id = "currency",
                        placeholder = "Currency (e.x. USD, INR etc.)",
                        value = currency ?? "",
                        isRequired = true,
                        label = "Currency",

                    },
                    new
                    {
                        type = "Input.ChoiceSet",
                        id = "category",
                        style = "Compact",
                        isMultiSelect = false,
                        value = category ?? "",
                        isRequired = true,
                        label = "Category",
                        choices = new List<object>
                        {
                            new { title = "Travel", value = "Travel" },
                            new { title = "Meals", value = "Meals" },
                            new { title = "Supplies", value = "Supplies" },
                            new { title = "Other", value = "Other" }
                        }
                    },
                    new
                    {
                        type = "Input.ChoiceSet",
                        id = "paymentMethod",
                        style = "Compact",
                        isMultiSelect = false,
                        value = paymentMethod ?? "",
                        isRequired = true,
                        label = "Payment Method",
                        choices = new List<object>
                        {
                            new { title = "Credit Card", value = "Credit Card"},
                            new { title = "Debit Card", value = "Debit Card" },
                            new { title = "Cash", value = "Cash" },
                            new { title = "Other", value = "Other" }

                        }

                    },
                    new
                    {
                    type = "Input.Text",
                    id = "fourDigits",
                    placeholder = "Last 4 Digits of Card",
                    value = fourDigits ?? "",
                    label = "Last 4 Digits of Card"
                },
                     new
                    {
                        type = "Input.Text",
                        id = "notes",
                        placeholder = "Notes (optional)",
                        value = notes ?? "",
                        label = "Notes"
                    }

                    },
                actions = new List<object>
        {
            new
            {
                type = "Action.Submit",
                title = "Done with Edit",
                data = new { action = "submitValidationForm" }
            }
        }
            };



            return JsonSerializer.Serialize(card);




        }



        //    [KernelFunction("GenerateConfirmationCard")]
        //    [Description("Generates a read-only confirmation adaptive card with all fields and submit/cancel buttons.")]
        //    public string GenerateConfirmationCard(string merchantName, string transactionDate, string total, string currency, string category, string paymentMethod, string fourDigits,string expenseType, string notes = "")
        //    {
        //        // Only show fourDigits if payment method is card
        //        bool showFourDigits = paymentMethod?.ToLowerInvariant().Contains("card") == true && !string.IsNullOrWhiteSpace(fourDigits);

        //        var body = new List<object>
        //{
        //    new { type = "TextBlock", text = "Confirm Expense Claim Submission", weight = "Bolder", size = "Medium" },
        //    new { type = "TextBlock", text = $"**Merchant Name:** {merchantName}" },
        //    new { type = "TextBlock", text = $"**Transaction Date:** {transactionDate}" },
        //    new { type = "TextBlock", text = $"**Total Amount:** {total}" },
        //    new { type = "TextBlock", text = $"**Currency:** {currency}" },
        //    new { type = "TextBlock", text = $"**Category:** {category}" },
        //    new { type = "TextBlock", text = $"**Expense Type:** {expenseType}" },
        //    new { type = "TextBlock", text = $"**Payment Method:** {paymentMethod}" },


        //};

        //        if (showFourDigits)
        //            body.Add(new { type = "TextBlock", text = $"**Last 4 Digits of Card:** {fourDigits}" });

        //        if (!string.IsNullOrWhiteSpace(notes))
        //            body.Add(new { type = "TextBlock", text = $"**Notes:** {notes}" });

        //        var card = new
        //        {
        //            type = "AdaptiveCard",
        //            version = "1.5",
        //            body = body,
        //            actions = new List<object>
        //    {
        //        new
        //        {
        //            type = "Action.Submit",
        //            title = "✅ Submit",
        //            data = new { action = "submitClaim" }
        //        },
        //        new
        //        {
        //            type = "Action.Submit",
        //            title = "❌ Cancel",
        //            data = new { action = "cancelClaim" }
        //        }
        //    }
        //        };

        //        return JsonSerializer.Serialize(card);
        //    }
        [KernelFunction("GenerateConfirmationCard")]
        [Description("Generates a read-only confirmation adaptive card with all fields and submit/cancel buttons.")]
        public string GenerateConfirmationCard(string merchantName, string transactionDate, string total, string currency, string category, string paymentMethod, string fourDigits, string expenseType, string notes = "")
        {
            bool showFourDigits = paymentMethod?.ToLowerInvariant().Contains("card") == true && !string.IsNullOrWhiteSpace(fourDigits);

            var body = new List<object>
    {
        new { type = "TextBlock", text = "🧾 Confirm Expense Submission", weight = "Bolder", size = "Large", wrap = true, spacing = "Medium" },
        new { type = "TextBlock", text = "Please review the details below before submitting your claim.", wrap = true, spacing = "Small", isSubtle = true },

        new { type = "TextBlock", text = "Expense Details", weight = "Bolder", spacing = "Medium", separator = true },
        new { type = "FactSet", facts = new List<object>
            {
                new { title = "Merchant Name:", value = merchantName },
                new { title = "Transaction Date:", value = transactionDate },
                new { title = "Total Amount:", value = total },
                new { title = "Currency:", value = currency },
                new { title = "Category:", value = category },
                new { title = "Expense Type:", value = expenseType },
                new { title = "Payment Method:", value = paymentMethod }
            }
        }
    };

            if (showFourDigits)
            {
                ((List<object>)((dynamic)body[3]).facts).Add(new { title = "Card Last 4 Digits:", value = fourDigits });
            }

            if (!string.IsNullOrWhiteSpace(notes))
            {
                body.Add(new { type = "TextBlock", text = "📝 Notes", weight = "Bolder", spacing = "Medium", separator = true });
                body.Add(new { type = "TextBlock", text = notes, wrap = true });
            }

            var card = new
            {
                type = "AdaptiveCard",
                version = "1.5",
                body = body,
                actions = new List<object>
        {
            new
            {
                type = "Action.Submit",
                title = "✅ Submit",
                style = "positive",
                data = new { action = "submitClaim" }
            },
            new
            {
                type = "Action.Submit",
                title = "❌ Cancel",
                style = "destructive",
                data = new { action = "cancelClaim" }
            }
        }
            };

            return JsonSerializer.Serialize(card);
        }


    }
}
