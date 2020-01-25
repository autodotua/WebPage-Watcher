using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPageWatcher.Data
{
    public class WebPageUpdate : IDbModel
    {
        public WebPageUpdate()
        {
        }

        public WebPageUpdate(int webPageID, byte[] newContent)
        {
            WebPage_ID = webPageID;
            Content = newContent;
            Time = DateTime.Now;
        }
        private int iD;
        public int ID
        {
            get => iD;
            set
            {
                iD = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ID)));
            }
        }
        private int webPage_ID;
        public int WebPage_ID
        {
            get => webPage_ID;
            set
            {
                webPage_ID = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WebPage_ID)));
            }
        }
        private DateTime time;
        public DateTime Time
        {
            get => time;
            set
            {
                time = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Time)));
            }
        }
        private byte[] content;
        public byte[] Content
        {
            get => content;
            set
            {
                content = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
            }
        }
        [Computed]
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;

        public WebPageUpdate Clone()
        {
            WebPageUpdate update = MemberwiseClone() as WebPageUpdate;

            return update;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}

