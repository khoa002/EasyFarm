﻿
/*///////////////////////////////////////////////////////////////////
<EasyFarm, general farming utility for FFXI.>
Copyright (C) <2013>  <Zerolimits>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
*/
///////////////////////////////////////////////////////////////////

using EasyFarm.Classes;
using EasyFarm.ViewModels;
using System.Windows.Controls;
using System.Linq;

namespace EasyFarm.Views
{
    /// <summary>
    /// Interaction logic for BattlesView.xaml
    /// </summary>
    public partial class BattlesView : UserControl
    {
        public BattlesView()
        {
            InitializeComponent();
        }
                
        /// <summary>
        /// Update the selected battle list in the BattlesViewModel. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void master_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext != null)
            {
                var vm = (BattlesViewModel)DataContext;
                
                if (e.AddedItems.Count > 0)
                {
                    vm.SelectedList = e.AddedItems[0] as BattleList;
                }
            }
        }
    }
}
