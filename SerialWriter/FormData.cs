﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialWriter
{
    public class FormData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FileNotPath
        {
            get { return (String.IsNullOrEmpty(this.FileName) ? "" : this.FileName.Substring(this.FileName.LastIndexOf("\\")+1)); }
           
        }

        private PortData[] portList;

        public PortData[] PortList
        {
            get { return this.portList; }
            set
            {
                this.portList = value;
                OnPropertyChanged("PortList");
            }
        }

        private PortData port;

        public PortData Port
        {
            get { return this.port; }
            set
            {
                this.port = value;
                OnPropertyChanged("Port");
            }
        }

        private int delay;

        public int Delay
        {
            get { return this.delay; }
            set
            {
                this.delay = value;
                OnPropertyChanged("Delay");
            }
        }

        private string fileName;
        public string FileName
        {
            get { return this.fileName; }
            set
            {
                this.fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        private bool isRepeat;
        public bool IsRepeat
        {
            get { return this.isRepeat; }
            set
            {
                this.isRepeat = value;
                OnPropertyChanged("IsRepeat");
            }

        }

        public bool Stop { get; set; }

      
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }


        }
    }

    public class PortData
    {
        public string Name { get; set; }
    }
}
