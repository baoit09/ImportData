   M S B A M L   `   `   `  ����`  \Xceed.Wpf.DataGrid, Version=3.5.9505.16400, Culture=neutral, PublicKeyToken=ba83ff368b7563c6S<clr-namespace:Xceed.Wpf.DataGrid;assembly=Xceed.Wpf.DataGridXceed.Wpf.DataGrid  5      P LDataGridFilterLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\Bclr-namespace:DataGridFilterLibrary;assembly=DataGridFilterLibraryDataGridFilterLibrary 5      E AImportData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:ImportData.Helpers.Control.PagingNavigator*ImportData.Helpers.Control.PagingNavigator 5         ImportData.Window1   5   	   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35I EWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=nullF 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      8x,http://schemas.microsoft.com/winfx/2006/xaml    5      LXceed<clr-namespace:Xceed.Wpf.DataGrid;assembly=Xceed.Wpf.DataGrid    5      ;ToolKit-http://schemas.microsoft.com/wpf/2008/toolkit  5      OfilterBclr-namespace:DataGridFilterLibrary;assembly=DataGridFilterLibrary  5      DLocal8clr-namespace:ImportData.Helpers.Control.PagingNavigator  5        � Title$(  "Import Data (Developeved By btran)��5      $
��Auto��60   $	��500��6>   .��5	      ]� .+�5
   
   9� $)��#Step 1: Select Table and Excel file��6   $��2q�6@   $	��Top=�6K    9� 
IsExpanded . .��5      � $��0��6    � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      �� 5      $
��Auto �6'    � RowDefinitions 5      �� 5      �� 5      �� 5      �� 5      .+�5      �� $��Table��6   $��2q�6)   $��Center=�64   �� 5      -   5        $��ComboBox_Table��5       �� SelectedValuePath$
 Name��6d    �� DisplayMemberPath$
 Name��6}   $��1��6�   $��2��6�   $��2q�6�   �� 5      $��
Excel File��6   $��1��6.   $��0��6;   $��2q�6K   $��Center=�6V   �� 5      -   5        $��TextBox_File��5      $��1��6.   $��1��6;   $��Center=�6K   $��Stretch=�6f   $��2q�6�    �� 
IsReadOnly . 6�   �� 5      -   5        $��Button_Browse��5      $��1��6.   $��2��6;   $��Center=�6K   $��Stretch=�6f   $��5q�6�   $��Browse��6�   6�   �� 5      $��Excel Sheet��6   $��2��6/   $��0��6<   $��2q�6L   $��Center=�6W   �� 5       -   5        $��ComboBox_Sheet��5       $��2��6d   $��1��6q   $��2��6�   $��Center=�6�   $��Stretch=�6�   $��2q�6�   $ Index��6�   $
 Name��6�   5!      5"      9� 5#   
   -   5        $��
Expander_2��5#   7   $��Step 2: Mapping��6   $��2q�6,   $	��Top=�6I    . .��5$      ]� ��. $��Stretch=�6.   .+�5%      ��  �� Orientation$ 
Horizontal=�6   $��Right=�66   $��Bottom=�6R   .+�5&      �� -   5        $��Button_Load��5&   ,   $
��Load��6   $��2q�6Y   �� 5'      -   5        $��Button_Save��5'   ,   $
��Save��6   $��2q�6Y   5(      )  #Microsoft.Windows.Controls.DataGrid  5)      -   5          Name$ DataGrid_Fields��5)   #   	   AutoGenerateColumns	 .  $
��Auto=�6Y   $
��Auto=�6�   
 �� AlternationCount$
 2��5*   #      AlternatingRowBackground
 �����   	RowHeight$ 25Q�6Y   $	��Top=�6h   .  (DataGridFilterLibrary.DataGridExtensions&   UseBackgroundWorkerForFiltering .    IsClearButtonVisible .    IsFilterVisible$
 TrueX�5-         ColumnHeaderStyle 5.      �� *��  �� TypeInTargetAssembly7  1DataGridFilterLibrary.DataGridHeaderFilterControl"   �� 
ResourceId$&  DataGridHeaderFilterControlStyle��+   Columns 52      3  -Microsoft.Windows.Controls.DataGridTextColumn  53      /  )Microsoft.Windows.Controls.DataGridColumn   Header$ Field��6N      
IsReadOnly . 4  .Microsoft.Windows.Controls.DataGridBoundColumn   Binding 65   �� *Name+6]     54      $ 	Data Type��6V    .  65   �� *DataTypeName+6i     55      $ DB Default Value��6`    .  65   �� *DBDefaulValueOrBinding+6z     56      $ 
Max Length��6S    .  65   �� *	MaxLength+6g   7  1Microsoft.Windows.Controls.DataGridCheckBoxColumn  57      $ Nullable��6X    .  69   �� *
IsNullable+6j     58      $ Primary Key��6Z    .  69   �� *IsPrimaryKey+6o   7  1Microsoft.Windows.Controls.DataGridComboBoxColumn  59      $ Columns��69   -	   5        "DataGridComboBoxColumn_Columns��   DisplayMemberPath$
 Name��5:   X      SelectedValuePath$ Index��6q      SelectedValueBinding 6!   ��  �� Path$ ExcelColumnIndex�6q     5;      $ 	Functions��6U    65   �� *FunctionIDs+6U     5<      $ Function Args��6V    65   �� *FunctionArgs+6V     5=      $ Default Value��6V    65   �� *DefaultValue+6V     5>      $ Unique��6V    69   �� *
IsUnique+6V   5@      5A      5B      9� 5C   
   $9��3Step 3: Read data from excel file, and preview data��6   $��2q�6P   $	��Top=�6[    . .��5D      ]� ��. $��Stretch=�6-   $��Stretch=�6I   .+�5E      �� -
   5        $��StackPanel_SheetInfo��5E   Q   $ 
Horizontal=�6   $
��Left=�66   $	��Top=�6m   .+�5F      �� $��Header Row Index��6    $��2q�68   $��Center=�6C   �� 5G      -   5        $��TextBox_HeaderRow��5G      $��Center=�67   $��50��6R   $��2q�6]   ��6h   �� *HeaderRowIndex+�� 5H      $��From Row��6    $��2q�60   $��Center=�6;   �� 5I      -   5        $��TextBox_FromRow��5I      $��Center=�65   $��50��6P   $��2q�6[   ��6f   �� *BeginRowIndex+�� 5J      $��To Row��6    $��2q�6.   $��Center=�69   �� 5K      -   5        $��TextBox_ToRow��5K      $��Center=�63   $��50��6N   $��2q�6Y   ��6d   �� *EndRowIndex+�� 5L      -   5        $��Button_Read��5L      $��Center=�6J   $��Stretch=�6e   $��2q�6�   $
��Read��6�   �� 5M      -   5        $��Button_Match��5M      $��Center=�61   $��Stretch=�6L   $��2q�6j   $��Match��6u   5N      �� 5O      $ 
Horizontal=�6   $��Right=�66   $��Bottom=�6R   .+�5P      �� -   5        $��Button_Preview��5P   /   $��Preview��6   $��2q�6b   �� 5Q      -   5        $��Button_DeleteData��5Q   3   $��Delete Data��6   $��2q�6L   �� 5R      -   5        $��Button_StepImport��5R   3   $��Step Import��6   $��2q�6L   5S      @	  :ImportData.Helpers.Control.PagingNavigator.PagingNavigator	  5T      -   5         	 Name$ PagingNavigator_Step3��5T   (   $��Bottom=�6G   6a     5U      -   5        $��DataGrid_Columns��5U   .   $��2q�6#   	 .  $
 2��6b   
 �����$ 25Q�6�   $	��200��6�   ��. $��Bottom=�6�      
IsReadOnly .  .  . $
 TrueX�5X       5Y      �� *�� "  $&  DataGridHeaderFilterControlStyle��+5\      5]      5^      5_      