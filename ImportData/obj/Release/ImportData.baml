   M S B A M L   `   `   `  ���� `  \Xceed.Wpf.DataGrid, Version=3.5.9505.16400, Culture=neutral, PublicKeyToken=ba83ff368b7563c6S<clr-namespace:Xceed.Wpf.DataGrid;assembly=Xceed.Wpf.DataGridXceed.Wpf.DataGrid  P LDataGridFilterLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\Bclr-namespace:DataGridFilterLibrary;assembly=DataGridFilterLibraryDataGridFilterLibrary E AImportData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:ImportData.Helpers.Control.PagingNavigator*ImportData.Helpers.Control.PagingNavigator    ImportData.Window1   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35I EWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=nullF 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     8x,http://schemas.microsoft.com/winfx/2006/xaml    LXceed<clr-namespace:Xceed.Wpf.DataGrid;assembly=Xceed.Wpf.DataGrid    ;ToolKit-http://schemas.microsoft.com/wpf/2008/toolkit  OfilterBclr-namespace:DataGridFilterLibrary;assembly=DataGridFilterLibrary  DLocal8clr-namespace:ImportData.Helpers.Control.PagingNavigator    � Title$(  "Import Data (Developeved By btran)��$
��Auto��$	��500�� � 	Resources    ExcelColumns��%   &	        S� .��]� .+�9� $)��#Step 1: Select Table and Excel file��$��2q�$	��Top=� 9� 
IsExpanded . .��� $��0�� � ColumnDefinitions �� $
��Auto ��� �� $
��Auto � � RowDefinitions �� �� �� �� .+��� $��Table��$��2q�$��Center=��� -   $��ComboBox_Table�� �� SelectedValuePath$
 Name�� �� DisplayMemberPath$
 Name��$��1��$��2��$��2q��� $��
Excel File��$��1��$��0��$��2q�$��Center=��� -   $��TextBox_File��$��1��$��1��$��Center=�$��Stretch=�$��2q� �� 
IsReadOnly . �� -   $��Button_Browse��$��1��$��2��$��Center=�$��Stretch=�$��5q�$��Browse���� $��Excel Sheet��$��2��$��0��$��2q�$��Center=��� -   $��ComboBox_Sheet��$��2��$��1��$��2��$��Center=�$��Stretch=�$��2q�$ Index��$
 Name��9� -   $��
Expander_2��$��Step 2: Mapping��$��2q�$	��Top=� . .��]� ��. $��Stretch=�.+���  �� Orientation$ 
Horizontal=�$��Right=�$��Bottom=�.+��� -   $��Button_Load��$
��Load��$��2q��� -   $��Button_Save��$
��Save��$��2q�)  #Microsoft.Windows.Controls.DataGrid  -   	  Name$	 DataGrid_Fields��
   AutoGenerateColumns
 .  $
��Auto=�$
��Auto=� �� AlternationCount$ 2��   AlternatingRowBackground
 �����   	RowHeight$ 25Q�$	��Top=�.  (DataGridFilterLibrary.DataGridExtensions&   UseBackgroundWorkerForFiltering .    IsClearButtonVisible .    IsFilterVisible$
 TrueX�$��Center=�$��Center=�   ColumnHeaderStyle �� *��  �� TypeInTargetAssembly7  1DataGridFilterLibrary.DataGridHeaderFilterControl"   �� 
ResourceId$&  DataGridHeaderFilterControlStyle��+ ��%       Columns 3  -Microsoft.Windows.Controls.DataGridTextColumn  /  )Microsoft.Windows.Controls.DataGridColumn   Header$ Field��   
IsReadOnly . 4  .Microsoft.Windows.Controls.DataGridBoundColumn   Binding �� *Name+  $ 	Data Type�� .  �� *DataTypeName+  $ DB Default Value�� .  �� *DBDefaulValueOrBinding+  $ 
Max Length�� .  �� *	MaxLength+7  1Microsoft.Windows.Controls.DataGridCheckBoxColumn  $ Nullable�� .  �� *
IsNullable+  $ Primary Key�� .  �� *IsPrimaryKey+  $ Identity�� .  �� *
IsIdentity+7  1Microsoft.Windows.Controls.DataGridComboBoxColumn  $ Columns��-	   "DataGridComboBoxColumn_Columns��   DisplayMemberPath$
 Name��   SelectedValuePath$ Index��   SelectedValueBinding ��  �� Path$ ExcelColumnIndex� �� UpdateSourceTrigger$ PropertyChanged=�7	  1Microsoft.Windows.Controls.DataGridTemplateColumn	  $ 	Functions�� 	  CellTemplate �� .R��� ���� $ FunctionIDs�	  $ Function Args�� �� .R��� ���� $ FunctionArgs�	  $ Default Value�� �� .R��� ���� $ DefaultValue�  $ Unique�� �� *
IsUnique+  $# Replace Empty By DefaultValue�� �� *ReplaceEmptyByDefaultValue+  $% Replace Empty By DBDefaultValue�� �� *ReplaceEmptyByDBDefaultValue+9� $9��3Step 3: Read data from excel file, and preview data��$��2q�$	��Top=� . .��]� ��. $��Stretch=�$��Stretch=�.+��� -
   $��StackPanel_SheetInfo��$ 
Horizontal=�$
��Left=�$	��Top=�.+��� $��Header Row Index��$��2q�$��Center=��� -   $��TextBox_HeaderRow��$��Center=�$��50��$��2q����� *HeaderRowIndex+�� $��From Row��$��2q�$��Center=��� -   $��TextBox_FromRow��$��Center=�$��50��$��2q����� *BeginRowIndex+�� $��To Row��$��2q�$��Center=��� -   $��TextBox_ToRow��$��Center=�$��50��$��2q����� *EndRowIndex+�� -   $��Button_Read��$��Center=�$��Stretch=�$��2q�$
��Read���� -   $��Button_Match��$��Center=�$��Stretch=�$��2q�$��Match���� $ 
Horizontal=�$��Right=�$��Bottom=�.+��� -   $��Button_Preview��$��Preview��$��2q��� -   $��Button_DeleteData��$��Delete Data��$��2q��� -   $��Button_StepImport��$��Step Import��$��2q�@
  :ImportData.Helpers.Control.PagingNavigator.PagingNavigator
  -    
 Name$ PagingNavigator_Step3��$��Bottom=�  -   $��DataGrid_Columns��$��2q�
 .  $ 2��
 �����$ 25Q�$	��200����. $��Bottom=�   
IsReadOnly .  .  . $
 TrueX� �� *�� "  $&  DataGridHeaderFilterControlStyle��+