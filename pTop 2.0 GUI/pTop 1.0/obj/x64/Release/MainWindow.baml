   M S B A M L   `   `   `  ���� O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  C ?pTop 2.0, Version=2.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:pToppTop +clr-namespace:pTop.classespTop.classes    pTop.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     4s&clr-namespace:System;assembly=mscorlib     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     X TXceed.Wpf.Toolkit, Version=3.4.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  localclr-namespace:pTop  #lcclr-namespace:pTop.classes    � Title$
  pTop�� � Icon$ images\ptop.png��$��MS Reference Sans Serif)�
�������$	��700��$
��1000�� � WindowState$ 	Maximized=�$	��500��$	��600�� � 	Resources    SizeTemplate �� TextAlignment  File_New_img  Folder_load_img  File_Save_img  File_SaveAs_img  Task_Run_img  Task_Stop_img  DTabControl1 �� 
TargetType � 	Resources �� 
TargetType �� Property	 �� Value
 J� SnapsToDevicePixels'  !System.Windows.VisualStateManager   VisualStateGroups%  System.Windows.VisualStateGroup   Name   States   System.Windows.VisualState   Name   
Storyboard �� 
TargetName �� TargetProperty	 W� To g� Duration	 �� To �� Stretch J� Opacity 
� GradientOrigin 
� RadiusX 
� RadiusY � Color � Offset � RowDefinitions �� Orientation K� IsItemsHost �� SelectedContent��%�  &	        &	 E     &	 {     &	 �     &	 �     &	 �     &	 ,    &	 b    �� .R��� $��Right=�$	��110��$ Right=����� *
FileSize+�� $��images\icons\filenew.gif��$��16��$��16���� $"��images\icons\folder_open.gif��$��16��$��16���� $��images\icons\save.gif���� $��images\icons\save_as.png���� $��images\icons\run.png��$��16��$��16���� $��images\icons\mm-Stop.ico��$��16��$��16���� " �� ���� '��       " ��.���  � ��	 �� " ��.R�� 
 .    $ CommonStates��.   $ 	MouseOver��. �� .��W� $ 
hoverShape��$ Opacity�$ 1X�$ 0:0:0.1D�  $ Normal��. �� .��W� $ 
hoverShape��$ Opacity�$ 0X�$ 0:0:0.1D�  $ SelectionStates��.   $ Selected��. �� .��W� $ buttonShape��$ Opacity�$ 1X�$ 0:0:0.3D�W� $ 
hoverShape��$ Opacity�$ 0X�$ 0:0:0.1D��� $ 
buttonText��$4 .(TextBlock.Foreground).(SolidColorBrush.Color)�$ WhiteX�$ 0:0:0.1D�  $ 
Unselected��. �� .��W� $ buttonShape��$ Opacity�$ 0X�$ 0:0:0.1D�W� $ 
hoverShape��$ Opacity�$ 0X�$ 0:0:0.1D�.+��� $��
hoverShape��$��32��$��0,0,1,0q�
 . $��0q�
�������._�F� =�����a� �oр�� �o� Ni dр�� ;Xрa� ;X� �Y d	
 . $
 Fill=�$��32��$	 0.3Q���
� $ 0.2,0.5&�$	 0.8Q�$	 0.8Q�.��� $ 	#ff5fa3f6��$ 0Q�� $ 	#ff0c55b9��$ 1Q��� $��buttonShape��$ 0Q����	#ffe9ecef$��0q�$��32��
 . ._�F� =�����a� �oр�� �o� Ni dр�� ;Xрa� ;X� �Y d	
 . $
 Fill=�$��32����
� $ 0.2,0.5&�$	 0.8Q�$	 0.8Q�.��� $ 	#ff5fa3f6��$ 0Q�� $ 	#ff0c55b9��$ 1Q��� $��
buttonText��-   $��15,0,5,0q�$��MS Reference Sans Serif)�$
��11pt(�
���   �$��Header��$��Center=�.R��  �� $��32 ��� .+��� ���	#ffe9ecef$��0��
�������$��1q�
 . �� $ 
Horizontal=� . $��0��$��0,0,-1,0q�
 . �� #��z $��1��$��15,0,0,0q�  J� CommandBindings  )  #System.Windows.Input.CommandBinding  -   !   Command$! lc:CustomCommands.New��  -   $! lc:CustomCommands.Open��  -   $! lc:CustomCommands.Run��  -   $! lc:CustomCommands.Rename��  -   $! lc:CustomCommands.Save��.��� -   $
��body�� �� $
��Auto ��� $
��Auto ��� �� $��30 �.+�o� $��0��.@�m� $
��File��.@�m� -   $��menuNew��$	��New��" m� InputGestureText$" Ctrl+N��m� -	   $��menuOpen��$
��Open��$" Ctrl+O��m� -
   $��menuSave��$
��Save��$" Ctrl+S���� m� -   $
��Exit��$" Alt+F4��m� $��_Options��.@�m� -   $��_Settings...��m� -   $!��_Meta Data Configuration...��m� -   $��helpItem��$
��Help��.@�m� -   $��About��c� -   $��toolbar��$��1��.@��� -   #��[ # J� RenderTransformOrigin$# 	0.36,0.32&�$ � ToolTip$ _� .��Create a new task(Ctrl+N)�� -   #��[ $# 	0.36,0.32&�$ _� .��Load a task(Ctrl+O)�� -   $��toolSave��#��[ $# 	0.36,0.32&�$ _� .��Save(Ctrl+S)�� -   $��toolRun��#��[ $# 	0.36,0.32&�  ButtonOpacity#��[ $ _� .��	Run(F5)�� -   $��toolStop��#��[ $# 	0.36,0.32&�#��[ $ _� .��Stop Task (F10)� $��2�� �� $��1* ��� $
��Auto ��� $	��150 �.+��� -   $	��tab��$��0��#���  .@��� -   $��FileTab��$��MS Data��% � FocusVisualStyle% Z� .���� $
��Auto=�$
��Auto=�$��0, 10, 0, 0q�.���  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��1��$ 
Horizontal=�$��Center=�.+��� $��	10,10,0,0q�$	��110��$
��Left=�$��Center=�MS Instrument :�� -   $��InstrumentChoices��$��Center=�$
��Left=�$��	15,10,0,0q�$	��120��& �� SelectedIndex& �� ' �� Path$' Instrument_index�.@��� .��HCD�� .��CID�� .��ETD�� .��EThcD�� .��ETciD�� .��UVPD� $��2��$��0,10,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��
10,10,10,0q�Data File List�� $��2��$ 
Horizontal=�$��	10,10,0,0q� 	 DFVisibility#��[	 .+��� -   $��dfnum��$
��Bold"�$��0���� $��
 File(s), ���� -   $��dfsize��$
��Bold"�$	��0.0���� -   $��	size_unit��$	�� GB���� $��1��$ 
Horizontal=�.+��� -   ( ��Name$( fileList��$��	10,10,0,0q�$	��520��$	��150�� 
 ListViewItemContainerStyle#��[
 $��Right=����� $' Data_file_list�) �� View) �� .P��� $��Files��* �� Width$	* 410��+ �� DisplayMemberBinding+ �� *
FilePath+�� $	* 110��$
��Size��, �� CellTemplate#, [  �� $��	50,12,0,0q�.+��� -   $��	addRawBtn��$��70��$
��Left=�$��10q�$��28��.��Add�� -   $��deleteRawBtn��$��10q�$��70��$
��Left=�$��28��.��Delete�� -   $��clearRawBtn��$��10q�$��70��$
��Left=�$��28��.��Clear9� -    $��pParseConfig��$��3��$��10q�- 9� 
IsExpanded- . ���� $��Center=�.��Data Extraction.���  �� $
��Auto ��� $
��Auto ��� $
��Auto �.+�� $��0�� �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$ 
Horizontal=�$��0,10,0,0q�.+��� $��Center=�$	��135��.��Precursors Detection�� $��1��$	��570��$��Center=�
���   ��� $��1��$ 
Horizontal=�$��45��.+��� $��Center=�$��20,0,0,0q�$	��118��$)$ #Full width of the isolation window.��.��Isolation Width :�� -!   $��isolation_width��$��Center=�$��75��$B$ <Isolation width should not be less than 2 or larger than 20.��.������ $%' Pparse_advanced.Isolation_width�. �� Mode$. TwoWay=�/ �� ValidationRules/ 5  /System.Windows.Controls.DataErrorValidationRule  5  /System.Windows.Controls.ExceptionValidationRule  .���� -"   $��MixSpec��$ 
Horizontal=�$��80,0,0,0q�.+��� -#   $��	MixSpecCB��$��Center=�0 d� 	IsChecked0 �� $!' Pparse_advanced.Mix_spectra�$. OneWay=��� $��Center=�.��Mixture Spectra� $��1�� �� $��40 ��� $��40 ��� $��40 �.+��� $��0��$ 
Horizontal=�$��0,10,0,0q�.+��� $��Center=�$	��135��.��Deconvolution�� $��1��$	��570��$��Center=�
���   ��� $��1��$ 
Horizontal=�$��20,0,0,0q�.+��� $��Center=�$	��118��$,$ &Maximum charge state of the precursor.��.��Maximum Charge :�� -$   $��
Max_charge��$��Center=�$��75��$A$ ;Maximum charge should not be less than 4 or larger than 50.��.������ $ ' Pparse_advanced.Max_charge�$. TwoWay=�/     .���� $��Center=�$��80,0,0,0q�$	��115��$J$ DUsed to match between theoretical and experimental isotope clusters.��.��M/Z Tolerance :�� -%   $��Mz_tolerance��$��Center=�$��75��$@$ :M/Z tolerance should not be less than 0 or larger than 30.��.������ $"' Pparse_advanced.Mz_tolerance�$. TwoWay=�/     .���� $��Center=�$��8,0,0,0q�.��ppm�� $��2��$ 
Horizontal=�$��20,0,0,0q�.+��� $��Center=�$	��118��$$$ Maximum mass of the precursor.��.��Maximum Mass :�� -&   $��Max_mass��$��Center=�$��75��$F$ @Maximum mass should not be less than 5000 or larger than 100000.��.������ $' Pparse_advanced.Max_mass�$. TwoWay=�/     .���� $��Center=�$��80,0,0,0q�$	��115��$U$ OUsed to compare the level of a desired signal to the level of background noise.��.��S/N Ratio :�� -'   $��SN_ratio��$��Center=�$��75��$;$ 5S/N ratio should not be less than 1 or larger than 3.��.������ $' Pparse_advanced.Sn_ratio�$. TwoWay=�/     .��� $��2�� �� $
��Auto ��� $
��Auto �.+��� $��0��$ 
Horizontal=�$��0,10,0,0q�.+��� $��Center=�$	��135��.��Place of Decimal�� $��1��$	��570��$��Center=�
���   ��� $��1��$ 
Horizontal=�$��40��$��20,0,0,0q�.+��� $��Center=�$	��118��$s$ mThis option specifies the decimal place of m/z ratio. Its value goes from 1 to 9, and the default value is 5.��.��M/Z :�� -(   $��	mzDecimal��$��75��$��Center=�& �� $&'  Pparse_advanced.Mz_decimal_index�.@��� .��1�� .��2�� .��3�� .��4�� .��5�� .��6�� .��7�� .��8�� .��9�� $��Center=�$��80,0,0,0q�$	��115��$s$ mThis option specifies the decimal place of intensity. Its value goes from 1 to 9, and the default value is 1.��.��Intensity :�� -)   $��IntensityDecimal��$��75��$��Center=�& �� $-' 'Pparse_advanced.Intensity_decimal_index�.@��� .��1�� .��2�� .��3�� .��4�� .��5�� .��6�� .��7�� .��8�� .��9�� $��0��$ 
Horizontal=�$��Center=�.+��� $��	10,10,0,0q�$	��110��$
��Left=�$��Center=�MS Data Format :�� -*   $��FormatChoices��$
��Left=�$��	15,10,0,0q�$	��120��$��Center=�& �� $' File_format_index�.@��� .��MGF�� .��RAW�� -+   $��IdentificationTab��$��Identification��% Z� .���� -,   $��IdentScroll��$
��Auto=�$
��Auto=�$��0,10,0,0q�.��� --   $��gridIndentify�� �� $
��Auto ��� $
��Auto �.+�9� -.   $��search��$��0��
 . - . ����   
LabelStyle#��[ $��Center=�.��Database Search.��� -/   $��
searchGrid�� �� $
��Auto ��� $
��Auto �.+�� $��0��$��0,5,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �1 � ColumnDefinitions1 �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$ 
Horizontal=�.+��� $��86��$��Center=�$
��Left=�.��
Database :�� -0   $��Database��$	��200��$��Center=�& �� $' Db_index��� -1   $��dbtip��$��10,0,0,0q�$��Center=�
���  ��  
DBtipStyle#��[ *�� $��1��$��0��$ 
Horizontal=�$��0,0,10,0q�.+��� $��Center=�$��5,0,0,0q�Precursor Tolerance :�� $��Center=�$��Right=�$��10,0,0,0q�.��±�� -2   $��PreTol��$��80��$��Center=�$=$ 7Hint: The value of the tolerance cannot be less than 0.��.������ $. TwoWay=�$' Ptl.Tl_value�/     .���� -3   $��	ptolUnits��$& 0��{�.  $��50��$��Center=�$��
10,10,0,10q�.@��� .��Da�� .��ppm�� $��1��$��1��$ 
Horizontal=�$��15,0,0,0q�.+��� $
��Left=�$��Center=�Fragment Tolerance :�� $��Center=�$��Right=�$��10,0,0,0q�.��±�� -4   $��FraTol��$��80��$��Center=�$=$ 7Hint: The value of the tolerance cannot be less than 0.��.������ $' Ftl.Tl_value�$. TwoWay=�/     .���� -5   $��	ftolUnits��$& 1��{�.  $��50��$��Center=�$��10,0,0,0q�.@��� .��Da�� .��ppm�� $��2��$��0��$ 
Horizontal=�$��0,0,10,0q�.+��� $��Center=�$��5,0,0,0q�$	��137��Max Truncated Mass :�� -6   $��maxTruncatedMass��$��80��$��Center=�$E$ ?Hint: The max mass allowed to be truncated on the N/C terminus.��.������ $. TwoWay=�$' MaxTruncatedMass�/     .���� $��Center=�$��5,0,0,0q�Da�� -7   $��SecondSearchSwitch��$��2��$��2��$ 
Horizontal=�$��15,0,0,0q�.+��� -8   $��SecondSearchCB��$��Center=�0 �� $' SecondSearch��� $��Center=�.��Second Search�� $��2��$��1��$ 
Horizontal=�$��Center=�$��15,0,0,0q�.+��� $	��110��$
��Left=�$��Center=�Search Mode :�� -9   $��
SearchMode��$
��Left=�$��Center=�$��15,0,0,0q�$��80��& �� $' SearchModeIndex�.@��� .��	Tag-Index�� .��	Ion-Index�� $��3��$��0��$ 
Horizontal=�$��0,0,10,5q�$��36��.+��� $	��142��$��Center=�$
��Left=�$G$ AMaximum number of variable modifications allowed on each protein.��.��Max PTM Positions :%  Xceed.Wpf.Toolkit.IntegerUpDown  -:   $��maxmod��$��80��$��Center=�$e$ _Maximum number of modifications (including variable and unexpected) allowed in each proteoform.��,  &Xceed.Wpf.Toolkit.Primitives.InputBase2   Text2 �� $. TwoWay=�$' Max_mod�/     �� $��3��$��1��$ 
Horizontal=�$��0,0,10,0q�.+��� $��Center=�$��15,0,0,0q�$	��110��Max Mod. Mass :�� -;   $��
maxModMass��$��80��$��Center=�$��15,0,0,0q�$h$ bHint: Maximum absolute value of the mass shift (in Dalton) of an modification. Default value: 500.��.������ $. TwoWay=�$' 
MaxModMass�/     .���� $��Center=�$��5,0,0,0q�Da�� $��3��$��2��$ 
Horizontal=�$��Center=�$��15,0,0,0q�.+��� $	��110��$
��Left=�$��Center=�Unexpected PTMs:�� -<   $��unexpectedMod��$
��Left=�$��Center=�$��15,0,0,0q�$��80��$Y$ SHint: Maximum number of unexpected modifications in a proteoform. Default value: 0.��& �� $' UnexpectedModNum�.@��� .��0�� .��1�� .��2�� -=   $��	ModConfig��$��1��$��2��$��Add Modification��.��� $��0,10,0,0q� �� $
��Auto ��� $
��Auto ��� $��42 �1 �� $
��Auto ��� $
��Auto ��� $	��100 ��� $
��Auto �.+��� $��0��$��0��$��	10,0,15,0q�$
��Auto��$��Center=�Fixed�� ->   $��selectedFixMod��$��0��$��1��$	��180��$	��110��$��0,5,0,0q����	#eef6f6f63 �� SelectionMode$3 Extended=����� $' Fix_mods��� $��0��$��2��$��Center=�.+��� -?   $��40��$��25��$��10q�.���� $
��Fill=�$��images/icons/arr_left.png���� -@   $��40��$��25��$��10q�.���� $
��Fill=�$ ��images/icons/arr_right.png���� $��1��$��0��$��	10,0,15,0q�$
��Auto��$��Center=�
Variable�� -A   $��selectedVarMod��$��1��$��1��$	��180��$	��110��$��0,10,0,0q����	#eef6f6f6$3 Extended=����� $' Var_mods��� $��1��$��2��$��Center=�.+��� -B   $��40��$��25��$��10q�.���� $
��Fill=�$��images/icons/arr_left.png���� -C   $��40��$��25��$��10q�.���� $
��Fill=�$ ��images/icons/arr_right.png���� -D   $��Modifications��$��0��$��2��$��3��$	��280��$	��230��$3 Extended=����	#aaF6F6F6
�������$��0,5,0,0q����� �� $��2��$��3��$ 
Horizontal=�$��5q�$��Center=�.+��� -E   $��	disAllMod��$��25��$��Center=��� $��Center=�Display All�� -F   $��Center=�$��	100,0,0,0q�$��60��$��25��$��Italic$�
�������.��	Edit...9� -G   $��filter��$��1��
 . - . ���� #��[ $��Center=�.��Result Filter.��� -H   $��
FilterGrid��1 �� $
��Auto ��� $
��Auto �.+��� $��0��$ 
Horizontal=�$��10q�.+��� $��FDR ≤ ��$��Center=��� -I   $��FDRValue��$��70��$��Center=�$7$ 1Hint: The value of FDR must be between 0 and 100.��.������ $' Filter.Fdr_value�$. TwoWay=�4 �� UpdateSourceTrigger$4 	LostFocus=�/     .���� $
�� %  ��$
��Left=�$��50��$��Center=��� $��1��$ 
Horizontal=�$��20,0,0,0q�.+��� -J   $��SeparateFilterCB��$��Center=�$w$ qHint: Filter separately for each input file. Otherwise, merge the results of all the input files and then filter.��0 �� $' Filter.SeparateFiltering��� $��Center=�.��Separate Filtering�� -K   $��QuantTab��$��Quantitation��% Z� .���� -L   $��quantScroll��$
��Auto=�$
��Auto=�$��0,10,0,0q�.��� -M   $��
V_MS1Quant�� �� $��45 ��� $
��Auto ��� $
��Auto �.+�� -N   $��Quant_Label��$��1�� �� $��50 ��� $
��Auto ��� $
��Auto �.+�� -O   $��multipLabels��$��1�� �� $��30 ��� $
��Auto ��� $
��Auto ��� $
��Auto �1 �� $	��100 ��� $	��250 ��� $��90 ��� $
��Auto �.+��� $��0��$��3��$ 
Horizontal=�$��Right=�.+��� -P   $��Center=�$��60��$��25��$��Italic$�
�������.��	Labels...�� -Q   $��
lightLabel��$��1��$��0��$
��Left=�$��Center=�.��Light Label :�� -R   $��mediumLabel��$��2��$��0��$
��Left=�$��Center=�.��Medium Label :�� -S   $��
heavyLabel��$��3��$��0��$
��Left=�$��Center=�.��Heavy Label :�� -T   $��selectedLightLabels��$��1��$��1��$��50��$	��240��$
��Left=����	#eeF6F6F6���� $' Labeling.Light_label��� -U   $��selectedMediumLabels��$��2��$��1��$��50��$	��240��$��0,10,0,0q�$
��Left=����	#eeF6F6F6���� $' Labeling.Medium_label��� -V   $��selectedHeavyLabels��$��3��$��1��$��50��$	��240��$��0,10,0,0q�$
��Left=����	#eeF6F6F6���� $' Labeling.Heavy_label��� -W   $( 	labellist��$��1��$��3��$��3��$	��210��$	��320�����	#efF6F6F6) �� .P��� $
��Name��$	* 110��+ �� *
Label_name+�� $��Value��+ �� *Label_element+�� -X   $��lightBtn��$��1��$��2��$��Center=�.+��� -Y   $��32��$��20��$��5q�  AddLBtnStyle#��[ .���� $
��Fill=�$��images/icons/arr_left.png���� -Z   $��32��$��20��$��5q�  RemoveLBtnStyle#��[ .���� $
��Fill=�$ ��images/icons/arr_right.png���� -[   $��	mediumBtn��$��2��$��2��$��Center=�.+��� -\   $��32��$��20��$��5q�  AddMBtnStyle#��[ .���� $
��Fill=�$��images/icons/arr_left.png���� -]   $��32��$��20��$��5q�  RemoveMBtnStyle#��[ .���� $
��Fill=�$ ��images/icons/arr_right.png���� -^   $��heavyBtn��$��3��$��2��$��Center=�.+��� -_   $��32��$��20��$��5q�  AddHBtnStyle#��[ .���� $
��Fill=�$��images/icons/arr_left.png���� -`   $��32��$��20��$��5q�  RemoveHBtnStyle#��[ .���� $
��Fill=�$ ��images/icons/arr_right.png���� $��0��$ 
Horizontal=�$��42��.+��� $��Multiplicity :��$
��Left=�$��Center=�$	��100���� -a   $��MultiplicityType��$	��125��$��Center=�& �� $!' Labeling.Multiplicity_index�.@��� -b   $��multiplicity1��.��1�� .��2�� .��39� -c   $��QuantAdvanced��$��2��- .  $��0,10,0,0q����� $��Center=�
Advanced.���  �� $
��Auto ��� $
��Auto ��� $
��Auto �1 �� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$��	10,10,0,0q�$��32��$ 
Horizontal=�.+��� $��Center=�$	��220��NUMBER_SCANS_HALF_CMTG :$ _� ���	#ffe9ecef.���� 5 �� 
IsReadOnly5 . $	��220��6 �� TextWrapping$
6 Wrap=����	#ffe9ecef$��0q�.��USThe number of MS1 scans along half of the chromatogram. Generally, it is 100 ~ 200.�� -d   $��number_scans_half_cmtg��$��Center=�$��75��
�������.������ $+' %Quant_advanced.Number_scans_half_cmtg�$. TwoWay=�/     .��$ _� ���	#ffe9ecef.���� 5 . $
6 Wrap=����	#ffe9ecef$��0q�.��:8You can only input integer number between 100 and 10000..���� $��1��$��0��$��	10,10,0,0q�$��32��$ 
Horizontal=�.+��� $��Center=�$	��220��PPM_FOR_CALIBRATION :$ _� ���	#ffe9ecef.���� 5 . $	��220��$
6 Wrap=����	#ffe9ecef$��0q�.��caThe mass biases (ppm level) of the peaks in MS1 scans. If you calibrated your Mass Spec, it is 0.�� -e   $��ppm_for_calibration��$��Center=�$��75��
�������.������ $(' "Quant_advanced.Ppm_for_calibration�$. TwoWay=�/     .���� $��1��$��1��$��	70,10,0,0q�$��32��$ 
Horizontal=�.+��� $��Center=�$	��220��PPM_HALF_WIN_ACCURACY_PEAK :$ _� ���	#ffe9ecef.���� 5 . $
6 Wrap=�$	��220�����	#ffe9ecef$��0q�.��usSimilar to precursor tolerance in the identification panel. pQuant will find the experimental peaks in this window.�� -f   $ ��ppm_half_win_accuracy_peak��$��Center=�$��75��
�������.������ $/' )Quant_advanced.Ppm_half_win_accuracy_peak�$. TwoWay=�/     .���� $��0��$��1��$��	70,10,0,0q�$��32��$ 
Horizontal=�.+��� $��Center=�$	��220��NUMBER_HOLE_IN_CMTG :$ _� ���	#ffe9ecef.���� 5 . $
6 Wrap=�$	��220�����	#ffe9ecef$��0q�.��wuIn general, the parameter is set to 2. It is used when pQuant find the start and the end points of the chromatograms.�� -g   $��number_hole_in_cmtg��$��75��$��Center=�& �� $(' "Quant_advanced.Number_hole_in_cmtg�.@��� .��1�� .��2�� .��3�� .��4�� .��5�� .��6�� .��7�� .��8�� .��9�� .��10�� -h   $��sample_rate_sp��$��2��$��0��$��32��$��	10,10,0,0q�$ 
Horizontal=�.+��� $��Center=�$	��310��(&TYPE_SAME_START_END_BETWEEN_EVIDENCE :$ _� ���	#ffe9ecef.���� 5 . $
6 Wrap=�$	��220�����	#ffe9ecef$��0q�.����The chromatograms of the light and heavy peptide have the dependent or the independent start and end points. In general, the sample is 1:1 mixed.�� -i   $*��$type_same_start_end_between_evidence��$	��180��$��Center=�& �� $9' 3Quant_advanced.Type_same_start_end_between_evidence�.@��� .��For 1:1 Mixed Samples�� .�� For 10:1 or 1:10 Mixed Samples�� .��Independent�� -j   $��calibriation_15N_sp��$��2��$��1��$��32��$��	70,10,0,0q�$ 
Horizontal=�.+��� $��Center=�#!ELEMENT_ENRICHMENT_CALIBRIATION :�� -k   $'��!ll_element_enrichment_calibration��$��75��$��Center=�& �� $6' 0Quant_advanced.Ll_element_enrichment_calibration�.@��� $ $ It is not 15N quantitation��.��none�� $P$ JIt is 15N quantitation, but the enrichment of 15N is fixed in element.ini.��.��15N�� $k$ eIt is 15N quantitation, pQuant will calculate the atomic percent enrichment of 15N for every peptide.��.��13C�� $��0��$ 
Horizontal=�$��0,10,0,0q�.+��� $��Type :��$
��Left=�$��Center=�$	��100���� -l   $��QuanType��$	��125��$��Right=�$��Center=�& �� $' Quantitation_type�.@��� .��
Label-None�� .��
Labeling�� -m   $��
SummaryTab��$��Summary��% Z� .���� -n   $��	SumScroll��$
��Auto=�$
��Auto=�$��0,0,0,0q�.���  �� $��28 ��� $
��Auto �.+�c� -o   $��0��$��28��7 `� IsLocked7 . .@��� -p   $��sumSave��#��[ $ _� .��Save the configuration.���� $��images/icons/save.gif���� -q   $��sumRun��$��5,0,5,0q�#��[ $ _� .��Start to run.���� $��images/icons/run.png���� -r   $��exp_pdf��$ _� .��)'Export this configuration to a pdf file.���� $!��images/icons/pdf_export.png��$��18��$��18��� $��1�� �� $
��Auto ��� $��60 �.+�� -s   8 �Name$8 all_summary��$��0�� �� $
��Auto ��� $
��Auto ��� $
��Auto �.+�9� -t   $��	FiRScroll��$��0��$��MS Data��- . .���� -u   $��
fileReport��$��20,0,0,0q�  SummaryListView#��[ ) �� .P��� $��Property��$	* 195��+ �� *	_property+�� $��Value��+ �� *_value+�� $��Check��$* 0��+ �� *_check+9� -v   $��SRScroll��$��1��$��Identification��- . .���� -w   $��searchReport��$��20,0,0,0q�#��[ ) �� .P��� $��Property��$	* 195��+ �� *	_property+�� $��Value��+ �� *_value+�� $��Check��$* 0��+ �� *_check+9� -x   $��QRScroll��$��2��$��Quantitation��- . .���� -y   $��quantReport��$��20,0,0,0q�#��[ ) �� .P��� $��Property��$	* 195��+ �� *	_property+�� $��Value��+ �� *_value+�� $��Check��$* 0��+ �� *_check+�� $��1��$ 
Horizontal=�$��Right=�$��0,0,20,0q�.+��� -z   $��saveBtn��$��80��$��30��$��0,0,20,0q�  ButtonStyle1#��[ .��Save�� -{   $��startBtn��$��80��$��30��$��0,0,20,0q�$��Right=�#��[ .��Start�� -|   $��stopBtn��$��80��$��30��$��Right=�#��[ .��Stop�� $��1��
�������$��Stretch=�$��2��9 �� ResizeBehavior$9 PreviousAndNext=��� -}   $��
outputInfo��$��2��
�������$
��Auto��$��Stretch=�
 . 
����������� $ 
Horizontal=�.+��� $��images/icons/output.png���� #��[ .��Output.���  �� $
��Auto ��� $��* �.+��� $��0��$��28��$ 
Horizontal=�.+��� -~   $��saveReportBtn��$��85��$��26��$
��Left=�$��Italic$�.��Save Report�� -   $��Output��$��1��
�������": �� VerticalScrollBarVisibility$
: Auto=�$; �� HorizontalScrollBarVisibility$; Hidden=�$
6 Wrap=�5 . $��Courier New)�
�������
���   �.���� -�   $��statusPanel�����#007acc$��3��$��0��$��3��$��Stretch=�$��25��
�������.@��� $ 
Horizontal=����#007acc.+��� -�   $��status1��Ready