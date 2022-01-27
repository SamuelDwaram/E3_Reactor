using System.ComponentModel;

namespace DragDropListBoxSample.Model
{
    public class SequenceBlock : INotifyPropertyChanged
    {
        private string blockOne;
        public string BlockOne
        {
            get => blockOne;
            set { blockOne = value; RaisePropertyChanged("BlockOne"); }
        }

        private string blockTwo;
        public string BlockTwo
        {
            get => blockTwo;
            set { blockTwo = value; RaisePropertyChanged("BlockTwo"); }
        }

        private string blockThree;
        public string BlockThree
        {
            get => blockThree;
            set { blockThree = value; RaisePropertyChanged("BlockThree"); }
        }

        private string blockFour;
        public string BlockFour
        {
            get => blockFour;
            set { blockFour = value; RaisePropertyChanged("BlockFour"); }
        }        

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
