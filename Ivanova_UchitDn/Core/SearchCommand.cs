﻿using System;
using System.Windows.Input;

namespace Ivanova_UchitDn.Core
{
    public class SearchCommand : ICommand
    {
        private Action searchDataDB;

        public SearchCommand(Action searchDataDB)
        {
            this.searchDataDB = searchDataDB;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            searchDataDB();
        }
    }
}
