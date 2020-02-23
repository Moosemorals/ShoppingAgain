using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace ShoppingAgain.TagHelpers
{
    [HtmlTargetElement(Attributes = "highlight")]
    public class HighlightTagHelper : TagHelper
    {
        public bool Highlight { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll("highlight");
            if (Highlight)
            {
                output.AddClass("highlight", HtmlEncoder.Default);
            }
        }

    }
}
