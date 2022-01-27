using Prism.Mvvm;
using System.Collections.Generic;

namespace E3.TrendsManager.Models
{
    public class TooltipContent : BindableBase
    {
        private IList<TooltipParameterAndValue> _content;
        public IList<TooltipParameterAndValue> Content
        {
            get => _content ?? (_content = new List<TooltipParameterAndValue>());
            set => SetProperty(ref _content, value);
        }
    }
}
