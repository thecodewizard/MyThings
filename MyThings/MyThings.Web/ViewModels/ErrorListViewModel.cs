using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyThings.Common.Models;

namespace MyThings.Web.ViewModels
{
    public class ErrorListViewModel
    {
        public List<Error> AllErrorsWarnings { get; set; }
        public List<Error> Errors { get; set; }
        public List<Error> Warnings { get; set; }
        public List<String> AutoCompleteSuggestionList { get; set; }
        public Boolean ErrorsOnly { get; set; }

        public List<ErrorCategory> ErrorCategories { get; set; }
        public List<String> ErrorCategoryStrings { get; set; }
    }
}