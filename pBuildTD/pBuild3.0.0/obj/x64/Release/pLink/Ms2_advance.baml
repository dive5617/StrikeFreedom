   M S B A M L   `   `   `  ���� 
  pBuild    pBuild.pLink.Ms2_advance   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  pLink.Ms2_advance��$	��450��$	��950�� � Icon$ ..\pBuild.ico�� � 	Resources    SingleClickEditing �� 
TargetType*  $System.Windows.Controls.DataGridCell��%   &	        �� "  .�-   .���  � ColumnDefinitions �� $��1* ��� $��1* �.+�� $��0�� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto � �� $��1* ��� $��3* �.+��� $��0��$��0��$��3q�Label:�� -    ��Name$ pep1_label_cbx��$��0��$��1��$��3q�.@��� .��None�� .��N15_Labeling�� $��1��$��0��SQ:�� -    ��Name$ pep1_sq_tbx��$��1��$��1��&   System.Windows.Controls.DataGrid  -     Name$ pep1_mod_dg��	   HeadersVisibility$	 Column=�
   HorizontalGridLinesBrush

 ����    VerticalGridLinesBrush
 ���� $��2��$��0��$��2��$��3q�   AutoGenerateColumns .     FrozenColumnCount$ 1��   RowHeaderWidth$ 0Q�����  �� Path$ 	Mod_Sites� �� ElementName$ adv_win��   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ #��   
IsReadOnly . 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Id+  $ AA�� .  �� *Aa+4  .System.Windows.Controls.DataGridComboBoxColumn  $ Modification��   Width5  /System.Windows.Controls.DataGridLengthConverter$	 180    	CellStyle# [     EditingElementStyle �� " ��.���  �� Property � �� �� Value �� $
 Mods���  �@��SelectedValue �� $	 Mod� �� UpdateSourceTrigger$ PropertyChanged=���  �@��IsDropDownOpen .    ElementStyle �� " ��.���  � �� �� $
 Mods���  �@��SelectedValue �� $	 Mod��� $��3��$��0��xLink Label:�� -   $ link_cbx��$��3��$��1��� $��1�� �� $
��Auto ��� $
��Auto ��� $��* ��� $
��Auto � �� $��1* ��� $��3* �.+��� $��0��$��0��$��3q�Label:�� -   $ pep2_label_cbx��$��0��$��1��$��3q�.@��� .��None�� .��N15_Labeling�� $��1��$��0��SQ:�� -   $ pep2_sq_tbx��$��1��$��1��  -   $ pep2_mod_dg��$	 Column=�

 ���� 
 ���� $��2��$��0��$��2��$��3q� .  $ 1��$ 0Q����� $ 	Mod_Sites�$ adv_win��   $ #�� .  �� *Id+  $ AA�� .  �� *Aa+  $ Modification��$	 180 # [   �� " ��.���  � �� �� $
 Mods���  �@��SelectedValue �� $	 Mod�$ PropertyChanged=���  �@��IsDropDownOpen .  �� " ��.���  � �� �� $
 Mods���  �@��SelectedValue �� $	 Mod��� $��3��$��0�� �� Orientation$ 
Horizontal=�$��2��$��0,0,332,-0.4q�.+��� $��Center=�xLink Position:�� -	   $ link_position_tbx��$��	0,0,0,0.4q�$��63���� $��3��$��1��$��26.6,0,0,-0.4q�$ 
Horizontal=�.+��� -
    ��Name$ 
normal_ckb�� d� 	IsChecked$
 trueY�$��4q�.��Normal�� -   $ internal_ckb��$��4q�.��
Internal�� -   $	��194��.��Update