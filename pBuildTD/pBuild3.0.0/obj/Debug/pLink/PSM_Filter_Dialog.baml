   M S B A M L   `   `   `  ����A  =pBuild, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:pBuild.pLinkpBuild.pLink  5   	   $    pBuild.pLink.PSM_Filter_Dialog   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:pBuild.pLink     5   	   -   5          � Title$  pLink.PSM_Filter_Dialog��5   	   $	��300��6)   $	��400��66    � Icon$ ..\pBuild.ico��6B   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,    � ColumnDefinitions 5   
   �� 5      $
��0.2* �6   6.   �� 5      $
��0.4* �6   6.   �� 5      $
��0.2* �6   6.   .+�5   
   �� $��0��6   $��0��6!   $��3q�61   Mixed Number6J   �� 5   
   -   5         ��Name$ filter_mix_num��5   :   $��0��6   $��1��6   $��3q�6/   $�� ��6R   .��6r    � ToolTip 5      �� 5      $"Filter By the Number of Precursors6@   .��5      5      �� 5   
   $��1��6   $��0��6!   $��3q�61   Title6C   �� 5   
   -   5        $ title_subStr_txt��5   :   $��1��6   $��1��6   $��3q�6/   $�� ��6T   .��6t    5      �� 5       Filter By Title SubString67   .��5!      5"      �� 5#   
   $��2��6   $��0��6!   $��3q�61   SQ6@   �� 5$   
   -   5        $ sq_subStr_txt��5$   :   $��2��6   $��1��6   $��3q�6/   $�� ��6Q   .��6q    5%      �� 5&      Filter By SQ SubString64   .��5'      5(      �� 5)   
   $��3��6   $��0��6!   $��3q�61   	Ratio ≤6E   �� 5*   
   -   5        $ 
ratio1_tbx��5*   :   $��3��6   $��1��6   $��3q�6/   $�� ��6N   .��6n    5+      �� 5,      Filter By Ratio6-   .��5-      5.      �� 5/   
   $��4��6   $��0��6!   $��3q�61   	Ratio ≥6E   �� 50   
   -   5        $ 
ratio2_tbx��50   :   $��4��6   $��1��6   $��3q�6/   $�� ��6N   .��6n    51      �� 52      Filter By Ratio6-   .��53      54      �� 55   
   -   5        $��3q�55      $��5��6   $��1��6)   .��6L   Filter6T   56      57      