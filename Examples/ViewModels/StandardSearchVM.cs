﻿using Caliburn.Micro;
/*
Copyright (c) 2010 Stephen P Ward and Joseph E Feser

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
using FeserWard.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Examples.ViewModels
{
    public class StandardSearchVM : PropertyChangedBase
    {
        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value != _selectedItem)
                {
                    _selectedItem = value;
                    NotifyOfPropertyChange(() => SelectedItem);
                }
            }
        }

        private object _selectedValue;
        public object SelectedValue { get { return _selectedValue; } set { if (value != _selectedValue) { _selectedValue = value; NotifyOfPropertyChange(() => SelectedValue); } } }

        private IIntelliboxResultsProvider _queryProvider;
        private Person currPerson;
        //private string currPersonName;

        public IIntelliboxResultsProvider QueryProvider { get { return _queryProvider; } private set { if (value != _queryProvider) { _queryProvider = value; this.NotifyOfPropertyChange(() => QueryProvider); } } }

        public StandardSearchVM(IIntelliboxResultsProvider provider)
        {
            QueryProvider = provider;
        }

        public Person CurrPerson { get => currPerson; set { currPerson = value; NotifyOfPropertyChange(() => CurrPerson); } }

        public string CurrPersonName
        {
            get => CurrPerson.name;
            set
            {
                //MessageBox.Show(CurrPerson.id.ToString());
                CurrPerson.name = value;
            }
        }

        public ObservableCollection<Person> DataGridSource
        {
            get
            {
                return new ObservableCollection<Person>() { new Person() { id = 1, name = "test1", sex = "male" }, new Person() { id = 2, name = "test2", sex = "female" } };
            }
        }
    }

    public class Person : PropertyChangedBase
    {
        private int id1;
        private string name1;
        private string sex1;

        public int id { get => id1; set { id1 = value; NotifyOfPropertyChange(() => id); } }

        public string name
        {
            get => name1;
            set
            {
                name1 = value;
                NotifyOfPropertyChange(() => name);
            }
        }

        public string sex
        {
            get => sex1;
            set
            {
                sex1 = value;
                NotifyOfPropertyChange(() => sex);
            }
        }
    }
}
