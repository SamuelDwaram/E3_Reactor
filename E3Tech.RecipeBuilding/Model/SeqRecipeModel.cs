using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.RecipeBuilding.Model
{
    public class SeqRecipeModel : BindableBase
    {
        public string FileLocation { get; set; }

        public string RecipeName { get; set; }

        public Guid RecipeGuidId { get; set; }

        private bool isExecuting;
        public bool IsExecuting
        {
            get
            {
                return isExecuting;
            }
            set
            {
                isExecuting = value;
                OnPropertyChanged();
            }
        }

        private bool isExecuted;
        public bool IsExecuted
        {
            get
            {
                return isExecuted;
            }
            set
            {
                isExecuted = value;
                OnPropertyChanged();
            }
        }

    }
}
