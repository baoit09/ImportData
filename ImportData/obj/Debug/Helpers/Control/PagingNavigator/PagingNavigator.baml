   M S B A M L   `   `   `  ����  
ImportData@    :ImportData.Helpers.Control.PagingNavigator.PagingNavigator   5      \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35I EWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=nullF 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      8x,http://schemas.microsoft.com/winfx/2006/xaml    5      .��5      �   � Orientation$  
Horizontal=�6   .+�5   
   �� -   5        $��StackPanel_ItemsPerPage��5      $y�	Collapsed=�64    �� Orientation$ 
Horizontal=�6K   $��Center=�6d   .+�5      �� -   5        $��ComboBox_ItemsPerPage��5      $��0,0,8,0q�64    �� 
IsEditable . $��50��6W   $��Center=�6�    �� SelectedItem 6�   ��  �� Path$ .� �� Mode$ TwoWay=�.@�5	      �� $��10��6   �� 5
      $��20��6   �� 5      $��50��6   �� 5      $	��100��6   5      �� 5      $��0,0,8,0q�6   $��Center=�6)   
Items/Page6P   5      d� 5   
   -   5        $��ToggleButton_Expaned��5      $��2,0,0,0q�63   $��%��6D   $��Center=�6P   $��20��6k   $��20��6v    � ToolTip$ Expanded��6�    � LayoutTransform 5      V� 5      .~�5      ��  �� ScaleX$ 1Q�6%   	 �� ScaleY$	 1Q�60   �� 5      
 �� AngleX$
 0Q�6$    �� AngleY$ 0Q�6/   �� 5       �� Angle$	 270Q�6&   T� 5       T� X$ 0Q�6)    T� Y$ 0Q�6/   5      5      �� 5   
   -   5        $��labelPageCount��5      $��Center=�6*   $��1��6F   $��2,0,2,0q�6O   �� 5   
   -   5        $��TextBlock_Pages��5      $��0,0,2,0q�6+   $��Center=�6<   	page(s)6`   �� 5   
   -   5        $��TextBlock_Total_Items��5      $��2,0,8,0q�61   $��Center=�6B   �� 5    
   -   5        $��buttonRefresh��5    W   $��2,0,0,0q�6   $��26��6"   $��21��60   $��26��6@   $��21��6K   $ Refesh��6l   ��5!      �� 5"      $��Stretch=�6   $��Stretch=�64   $Q��Kpack://application:,,,/ImportData;component/Resources/Navigator/Refresh.png��6R   5$      �� 5%   
   -   5        $��buttonFirst��5%   W   $��8,0,0,0q�6   $��26��6"   $��21��60   $��26��6@   $��21��6K   $ Go to first page��6j   ��5&      �� 5'      $��Stretch=�6   $��Stretch=�64   $O��Ipack://application:,,,/ImportData;component/Resources/Navigator/First.png��6R   5)      �� 5*   
   -	   5        $��buttonPrevious��5*   V   $��2,0,0,0q�6   $��26��6"   $��21��60   $��26��6?   $��21��6J   $ Go to previous page��6l   ��5+      �� 5,      $��Stretch=�6   $��Stretch=�64   $M��Gpack://application:,,,/ImportData;component/Resources/Navigator/Pre.png��6R   5.      �� 5/   
   -
   5        $��textBoxCurrent��5/   %   $��8,0,0,0q�6   $��Center=�6;   	Current6_   �� 50   
   -   5        $��textBoxCurrentPage��50   #   $��2,0,0,0q�6   $��30��6l   $��Center=�6z   $��Center=�6�   $��1��6�    �� AutoWordSelection .  6�   �� 51   
   -   5        $��buttonGo��51   W   $��0,0,6,0q�6   $��26��6"   $��21��60   $��26��6@   $��21��6K   ��52      �� 53      $��Stretch=�6   $��Stretch=�64   $L��Fpack://application:,,,/ImportData;component/Resources/Navigator/Go.png��6R   55      �� 56   
   -   5        $��
buttonNext��56   W   $��2,0,0,0q�6   $��26��6"   $��21��60   $��26��6@   $��21��6K   $ Go to next page��6i   ��57      �� 58      $��Stretch=�6   $��Stretch=�64   $N��Hpack://application:,,,/ImportData;component/Resources/Navigator/Next.png��6R   5:      �� 5;   
   -   5        $��
buttonLast��5;   W   $��2,0,0,0q�6   $��26��6"   $��21��60   $��26��6@   $��21��6K   $ Go to last page��6i   ��5<      �� 5=      $��Stretch=�6   $��Stretch=�64   $N��Hpack://application:,,,/ImportData;component/Resources/Navigator/Last.png��6R   5?      5A      5B      