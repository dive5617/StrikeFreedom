   M S B A M L   `   `   `  ���� 
  pBuild    pBuild.PSM_Filter_Dialog   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  PSM_Filter_Dialog��$	��440��$	��400�� � Icon$ 
pBuild.ico�� � 
ResizeMode$ NoResize=� � Left$	 380��
 � Top$ 20��.��� $��	0,0,0,-37q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��0.3* ��� $
��0.4* ��� $
��0.2* �.+��� $��0��$��0��$��3q�Mixed Number�� -    ��Name$ filter_mix_num��$��0��$��1��$��3q�$�� ��.�� � ToolTip �� $"Filter By the Number of Precursors.���� $��1��$��0��$��3q�Title�� -   $ title_subStr_txt��$��1��$��1��$��3q�$�� ��.�� �� Filter By Title SubString.���� -   	 ��Name$	 	title_cbb��$��1��$��2��$��3q�.@��� 
 �� 
IsSelected
 . .��Normal�� .��	Regular �� 1/Switch between normal search and regular search�� $��2��$��0��$��3q�SQ�� -   $ sq_subStr_txt��$��2��$��1��$��3q�$�� ��.�� �� Filter By SQ SubString.���� -   $	 sq_cbb��$��2��$��2��$��3q�.@��� 
 . .��Normal�� .��	Regular �� 1/Switch between normal search and regular search�� $��3��$��0��$��3q�Modification�� -   $	 mods_comboBox��$��3��$��1��$��3q� �� KIFilter by Modification. You can use "Ctrl+V" to filter modification name.�� -   $	 mods_comboBox2��$��3��$��2��$��3q� �� Filter by Modification�� -	    ��Name$ warning_tbk��$y�	Collapsed=�$��1��$��5��$��1��$��3q�
������ $
��Wrap=�
���  ���� $��4��$��0��$��3q�
Specific�� -
   $	 specific_comboBox��$��4��$��1��$��3q� �� Filter by Specific.@��� 
 . .��
Show All�� .��
Specific�� .��
N-specific�� .��
C-specific�� .��Non-specific�� $��5��$��0��$��3q�
Label Name�� -   $	 label_comboBox��$��5��$��1��$��3q� �� Filter by Label Name�� -   $ 	ratio_tbk��$��6��$��0��$��3q�$y�	Collapsed=�	Ratio ≤�� -   $ 	ratio_tbx��$��6��$��1��$��3q�$�� ��$y�	Collapsed=�.�� �� Filter By Ratio.���� -   $ 
ratio_tbk2��$��7��$��0��$��3q�$y�	Collapsed=�	Ratio ≥�� -   $ 
ratio_tbx2��$��7��$��1��$��3q�$�� ��$y�	Collapsed=�.�� �� Filter By Ratio.���� -   $ 	sigma_tbk��$��8��$��0��$��3q�$y�	Collapsed=�	Sigma ≤�� -   $ 	sigma_tbx��$��8��$��1��$��3q�$�� ��$y�	Collapsed=�.�� �� Filter By Sigma.���� -   $ 
sigma_tbk2��$��9��$��0��$��3q�$y�	Collapsed=�	Sigma ≥�� -   $ 
sigma_tbx2��$��9��$��1��$��3q�$�� ��$y�	Collapsed=�.�� �� Filter By Sigma.���� -   $ 
target_tbk��$��10��$��0��$��3q�	Target:�� -   $	 target_comboBox��$��10��$��1��$��3q� �� Filter by Target or Decoy.@��� .��
Show All�� 
 . .��Target�� .��Decoy�� $��11��$��0��$��3q�Contaminants:�� -    ��Name$ con_need_cbx��$��11��$��1��$��3q�$��Center=��� $��12��$��0��$��3q�	Unique:�� -   $ 
unique_cbx��$��12��$��1��$��3q�$��Center=�� $��13��$��0��$��2�� �� $��* ��� $��* �.+��� -   $��3q�$��0��.��	Display�� -   $��3q�$��1��.��Filter� $��14��$��0��$��3�� �� $��* ��� $��* ��� $��* �.+��� $��3q�$��0��Mod�� -   $ modTxt2��$��3q�$��1���� -   $��3q�$��2��.��Filter