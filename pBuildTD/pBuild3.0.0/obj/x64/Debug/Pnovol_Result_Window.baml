   M S B A M L   `   `   `  ����
  pBuild!    pBuild.Pnovol_Result_Window   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Pnovol_Result��5   	   $	��300��6   $	��600��6,    � Icon$ 
pBuild.ico��68   .��5      � .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ pNovo_result_grid��5   �      HeadersVisibility$ Column=�5         HorizontalGridLinesBrush
 ����    VerticalGridLinesBrush
 ����    AutoGenerateColumns .     FrozenColumnCount$ 1��6�      
IsReadOnly . 	   SelectionMode$	 Extended=�5   !   
   RowHeaderWidth$
 0Q�6X      ClipboardCopyMode$ IncludeHeader=�6k      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5	      ,  &System.Windows.Controls.DataGridColumn   Header$ #��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 50 6G   1  +System.Windows.Controls.DataGridBoundColumn   Binding 60   �� *ID+6G     5
      $ SQ��6%   $	 260 6H    61   �� *SQ+6H     5      $ Modification��6%   $	 180 6`    6;   �� *Modification_Str+6`     5      $ Score��6%   $	 100 6N    64   �� *Score+6N   5      5      5      