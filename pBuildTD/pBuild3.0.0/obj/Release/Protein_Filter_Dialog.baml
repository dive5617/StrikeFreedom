   M S B A M L   `   `   `  ���� 
  pBuild"    pBuild.Protein_Filter_Dialog   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Protein_Filter_Dialog��$	��300��$	��300�� � Icon$ 
pBuild.ico�� � 
ResizeMode$ NoResize=� � Left$	 100��
 � Top$	 100��.���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��0.4* ��� $
��0.6* �.+��� $��0��$��0��$��3q�AC�� -    ��Name$ protein_filter_AC_tb��$��0��$��1��$��3q�.�� � ToolTip �� Filter By AC SubString.���� $��1��$��0��$��3q�DE�� -   $ protein_filter_DE_tb��$��1��$��1��$��3q�.�� �� Filter By DE SubString.���� $��2��$��0��$��3q�Group�� -   $ protein_filter_Group_tb��$��2��$��1��$��3q�.�� �� Filter By Group SubString.���� $��3��$��0��$��3q�Peptide Number�� -   $ protein_peptide_number_tb��$��3��$��1��$��3q�.�� �� Filter By Peptide Number.���� $��4��$��0��$��3q�	Sq Number�� -   $ protein_sq_number_tb��$��4��$��1��$��3q�.�� �� Filter By Sq Number.���� -   	 ��Name$	 
target_tbk��$��5��$��0��$��3q�	Target:�� -   
 ��Name$
 target_comboBox��$��5��$��1��$��3q� �� Filter by Target or Decoy.@��� .��
Show All��  �� 
IsSelected . .��Target�� .��Decoy�� $��6��$��0��$��3q�Contaminants:�� -	    ��Name$ con_need_cbx��$��6��$��1��$��3q�$��Center=��� -
   $��7��$��1��$��3q�.��Filter