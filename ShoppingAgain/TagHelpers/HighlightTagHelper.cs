using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ShoppingAgain.TagHelpers
{
    [HtmlTargetElement(Attributes = nameof(Highlight))]
    public class HighlightTagHelper : TagHelper
    {
        public bool Highlight { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Highlight)
            {
                output.AddClass("highlight", HtmlEncoder.Default);
            }
        }

    }
}
