using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ShoppingAgain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingAgain.TagHelpers
{

    [HtmlTargetElement("Icon", TagStructure = TagStructure.WithoutEndTag)]
    public class IconTagHelper : TagHelper
    {
        private static readonly string SVG_NS = "http://www.w3.org/2000/svg";
        private IUrlHelperFactory _urlHelperFactory;
        private IActionContextAccessor _actionContextAccessor;
        private IWebHostEnvironment _env;
        public string Src { get; set; }
        public int Width { get; set; } = 20;
        public int Height { get; set; } = 20;
        public string @Class { get; set; }

        public IconTagHelper(IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IWebHostEnvironment environment)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _env = environment;
        }

        private string ServerPath(string src)
        {
            string imgPath = _urlHelperFactory
                .GetUrlHelper(_actionContextAccessor.ActionContext)
                .Content(src);
            if (imgPath.StartsWith("/"))
            {
                imgPath = imgPath.Substring(1);
            }
            string webRoot = _env.WebRootPath;
            return Path.GetFullPath(Path.Combine(webRoot, imgPath));
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
           string svg = File.ReadAllText(ServerPath(Src));

            output.TagName = "svg";
            output.Attributes.Add("xmlns", SVG_NS);
            output.Attributes.Add("width", Width);
            output.Attributes.Add("height", Height);
            if (!string.IsNullOrEmpty(Class))
            { 
                output.Attributes.Add("class", @Class);
            }
            output.Attributes.Add("version", "1.1");
            output.Content.SetHtmlContent(new HtmlString(svg));
            output.TagMode = TagMode.StartTagAndEndTag;

        }
    }
}
