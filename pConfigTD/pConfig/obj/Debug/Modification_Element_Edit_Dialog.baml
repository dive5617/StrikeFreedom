   M S B A M L   `   `   `  ����  pConfig.    (pConfig.Modification_Element_Edit_Dialog   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$(  "Modification's Element Information��5   	   $	��400��64   $	��350��6A    � Icon$ images\pConfig.ico��6M    � 
ResizeMode$ NoResize=�6g   .��5      � $��	0,0,-30,0q�6    � RowDefinitions 5   
   �� 5      $	��55* �6   6+   �� 5      $
��255* �6   6,   �� 5	      $	��62* �6   .+�5   
   � $��0,0,0,0q�6   .+�5      �� -   5         ��Name$ element_listView��5       �� DisplayMemberPath$
 Name��5      $
��Left=�6(   $��24��6C   $��
102,15,0,0q�6O   $	��121��6c   ��5   1   ��  �� Path$ elements�5      �� 5      $
��Left=�6   $��Center=�63   $��25��6N   $��	0,15,0,-1q�6Z    �� Padding$ 4q�6m   $
��Wrap=�6y    �� TextAlignment$ Center=�6�   $��79��6�   	 J� RenderTransformOrigin$	 	0.5,0.062&�6�   �� 6�   
 � Language$
 zh-cn�6�   $�Element��6�   6�   �� 5      -   5         ��Name$ Add_element_btn��5      $��40��6.   $��25��69   $��+��6E   $��246,14,64,0q�6Q   $	 0.725,0.238&�6f   6�   5      �� 5   
   $
��Auto=�6   $��1��6:   .��5      � -   5         �Name$ element_grid��5      $��0,0,0,0q�6)   5      � 5   
   $��2��6   .+�5      �� -   5        $��Apply��5      $
��Left=�6%   $��28��6@   $��
122,10,0,0q�6L   $	��Top=�6`   $��92��6x   6�   5      5      5      