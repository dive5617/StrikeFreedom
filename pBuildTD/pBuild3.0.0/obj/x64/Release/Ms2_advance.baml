   M S B A M L   `   `   `  ���� 
  pBuild    pBuild.Ms2_advance   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     �Name$  adv_win�� � Title$ MS2_advance��$	��450��$	��950�� � Icon$ 
pBuild.ico�� � Left$	 100��
 � Top$	 100�� � 	Resources    SingleClickEditing �� 
TargetType*  $System.Windows.Controls.DataGridCell��%   &	        �� "  .�-   .���  � ColumnDefinitions �� $	��380 �$	��380���� $
��Auto ��� $��* �.+��� $��0��$��Main Configure��$��2��.���� $
��Auto=�$
��Auto=�.���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* � �� $
��Auto �$��50���� $
��Auto ��� $
��Auto �.+��� $��0��$��0��Candidate Number�� -   	 ��Name$	 cand_cb��$��0��$��1��$
��10,0q�.@��� 
 �� 
IsSelected
 . .��1�� .��2�� .��3�� .��4�� .��5�� .��6�� .��7�� .��8�� .��9�� .��10�� $��1��$��0��$��10q�SEQ�� -    ��Name$	 seq��$��1��$��1��$	��200��$��10q�.��.���� $��2��$��0��$��10q�TOL�� -   $	 tol��$��2��$��1��$��10q�$��50��$
��Left=��� -   $	 ppm_chk��$��2��$��1��$
��Left=�$��70,10,10,10q�$��60��.@��� 
 . .��ppm�� .��Da�� -    ��Name$ preprocess_cbx��$��3��$��0��$��2��$��10q�$��Center=�$
��Left=�$	��100��.��
Preprocess�� $��3��$��1��$��70,10,10,10q�$��50��$
��Left=�$��Center=�Label:�� -   $	 aa_index_comboBox��$��3��$��1��$��95,10,10,10q�$��80���� -	   $��3��$��2��$��10q�.��Update&   System.Windows.Controls.DataGrid     HeadersVisibility$ Column=�   HorizontalGridLinesBrush
 ����    VerticalGridLinesBrush
 ���� $��4��$��0��$��3��$��10q�   AutoGenerateColumns .     FrozenColumnCount$ 1��   RowHeaderWidth$ 0Q�����  �� Path$ 	Mod_Sites� �� ElementName$ adv_win��   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ #��   
IsReadOnly . 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Id+  $ AA�� .  �� *Aa+4  .System.Windows.Controls.DataGridComboBoxColumn  $ Modification��   Width5  /System.Windows.Controls.DataGridLengthConverter$	 180    	CellStyle# [     EditingElementStyle �� " ��.���  �� Property � �� �� Value �� $
 Mods���  �@��SelectedValue �� $	 Mod� �� UpdateSourceTrigger$ PropertyChanged=���  �@��IsDropDownOpen .    ElementStyle �� " ��.���  � �� �� $
 Mods���  �@��SelectedValue �� $	 Mod�  $ Mod_Flag��$ 80 # [   �� " ��.���  � �� �� $ 	Mod_Flags���  �@��SelectedValue �� $ Mod_Flag���  �@��IsDropDownOpen .  �� " ��.���  � �� �� $ 	Mod_Flags���  �@��SelectedValue �� $ Mod_Flag�  $
 Mass�� .  �� *Mass+  $ Element�� .  �� *	Element+�� $��1��
�������$��Stretch=�$��5��  �� ResizeBehavior$  PreviousAndNext=��� $��2��$��Ion Type Select��.���� $
��Auto=�$
��Auto=�.��� -
   ! �Name$
! grid��