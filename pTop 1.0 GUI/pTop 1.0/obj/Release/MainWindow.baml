   M S B A M L   `   `   `  ���� O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  C ?pTop 1.2, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:pToppTop +clr-namespace:pTop.classespTop.classes    pTop.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     4s&clr-namespace:System;assembly=mscorlib     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     X TXceed.Wpf.Toolkit, Version=2.3.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  localclr-namespace:pTop  #lcclr-namespace:pTop.classes    � Title$
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
��Exit��$" Alt+F4��m� $��_Options��.@�m� -   $!��_Meta Data Configuration...��m� -   $��helpItem��$
��Help��.@�m� -   $��About��c� -   $��toolbar��$��1��.@��� -   #��[ # J� RenderTransformOrigin$# 	0.36,0.32&�$ � ToolTip$ _� .��Create a new task(Ctrl+N)�� -   #��[ $# 	0.36,0.32&�$ _� .��Load a task(Ctrl+O)�� -   $��toolSave��#��[ $# 	0.36,0.32&�$ _� .��Save(Ctrl+S)�� -   $��toolRun��#��[ $# 	0.36,0.32&�  ButtonOpacity#��[ $ _� .��	Run(F5)�� -   $��toolStop��#��[ $# 	0.36,0.32&�#��[ $ _� .��Stop Task (F10)� $��2�� �� $��1* ��� $
��Auto ��� $	��150 �.+��� -   $	��tab��$��0��#���  .@��� -   $��FileTab��$��MS Data��% � FocusVisualStyle% Z� .���� $
��Auto=�$
��Auto=�$��0, 10, 0, 0q�.���  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��1��$ 
Horizontal=�$��Center=�.+��� $��	10,10,0,0q�$	��110��$
��Left=�$��Center=�MS Instrument :�� -   $��InstrumentChoices��$��Center=�$
��Left=�$��	15,10,0,0q�$	��120��& �� SelectedIndex& �� ' �� Path$' Instrument_index�.@��� .��HCD�� .��CID�� .��ETD�� .��UVPD� $��2��$��0,10,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��
10,10,10,0q�Data File List�� $��2��$ 
Horizontal=�$��	10,10,0,0q� 	 DFVisibility#��[	 .+��� -   $��dfnum��$
��Bold"�$��0���� $��
 File(s), ���� -   $��dfsize��$
��Bold"�$	��0.0���� -   $��	size_unit��$	�� GB���� $��1��$ 
Horizontal=�.+��� -   ( ��Name$( fileList��$��	10,10,0,0q�$	��520��$	��150�� 
 ListViewItemContainerStyle#��[
 $��Right=����� $' Data_file_list�) �� View) �� .P��� $��Files��* �� Width$	* 410��+ �� DisplayMemberBinding+ �� *
FilePath+�� $	* 110��$
��Size��, �� CellTemplate#, [  �� $��	50,12,0,0q�.+��� -   $��	addRawBtn��$��70��$
��Left=�$��10q�$��28��.��Add�� -   $��deleteRawBtn��$��10q�$��70��$
��Left=�$��28��.��Delete�� -   $��clearRawBtn��$��10q�$��70��$
��Left=�$��28��.��Clear9� -   $��pParseConfig��$��3��$��10q�- 9� 
IsExpanded- . ���� $��Center=�.��Data Extraction.���  �� $
��Auto ��� $
��Auto ��� $
��Auto �.+�� $��0�� �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$ 
Horizontal=�$��0,10,0,0q�.+��� $��Center=�$	��135��.��Precursors Detection�� $��1��$	��570��$��Center=�
���   ��� $��1��$ 
Horizontal=�$��45��.+��� $��Center=�$��20,0,0,0q�$	��118��$)$ #Full width of the isolation window.��.��Isolation Width :�� -    $��isolation_width��$��Center=�$��75��$B$ <Isolation width should not be less than 2 or larger than 20.��.������ $%' Pparse_advanced.Isolation_width�. �� Mode$. TwoWay=�/ �� ValidationRules/ 5  /System.Windows.Controls.DataErrorValidationRule  5  /System.Windows.Controls.ExceptionValidationRule  .���� -!   $��MixSpec��$ 
Horizontal=�$��80,0,0,0q�.+��� -"   $��	MixSpecCB��$��Center=�0 d� 	IsChecked0 �� $!' Pparse_advanced.Mix_spectra�$. OneWay=��� $��Center=�.��Mixture Spectra� $��1�� �� $��40 ��� $��40 ��� $��40 �.+��� $��0��$ 
Horizontal=�$��0,10,0,0q�.+��� $��Center=�$	��135��.��Deconvolution�� $��1��$	��570��$��Center=�
���   ��� $��1��$ 
Horizontal=�$��20,0,0,0q�.+��� $��Center=�$	��118��$,$ &Maximum charge state of the precursor.��.��Maximum Charge :�� -#   $��
Max_charge��$��Center=�$��75��$A$ ;Maximum charge should not be less than 4 or larger than 50.��.������ $ ' Pparse_advanced.Max_charge�$. TwoWay=�/     .���� $��Center=�$��80,0,0,0q�$	��115��$J$ DUsed to match between theoretical and experimental isotope clusters.��.��M/Z Tolerance :�� -$   $��Mz_tolerance��$��Center=�$��75��$@$ :M/Z tolerance should not be less than 0 or larger than 30.��.������ $"' Pparse_advanced.Mz_tolerance�$. TwoWay=�/     .���� $��Center=�$��8,0,0,0q�.��ppm�� $��2��$ 
Horizontal=�$��20,0,0,0q�.+��� $��Center=�$	��118��$$$ Maximum mass of the precursor.��.��Maximum Mass :�� -%   $��Max_mass��$��Center=�$��75��$F$ @Maximum mass should not be less than 5000 or larger than 100000.��.������ $' Pparse_advanced.Max_mass�$. TwoWay=�/     .���� $��Center=�$��80,0,0,0q�$	��115��$U$ OUsed to compare the level of a desired signal to the level of background noise.��.��S/N Ratio :�� -&   $��SN_ratio��$��Center=�$��75��$;$ 5S/N ratio should not be less than 1 or larger than 3.��.������ $' Pparse_advanced.Sn_ratio�$. TwoWay=�/     .��� $��2�� �� $
��Auto ��� $
��Auto �.+��� $��0��$ 
Horizontal=�$��0,10,0,0q�.+��� $��Center=�$	��135��.��Place of Decimal�� $��1��$	��570��$��Center=�
���   ��� $��1��$ 
Horizontal=�$��40��$��20,0,0,0q�.+��� $��Center=�$	��118��$s$ mThis option specifies the decimal place of m/z ratio. Its value goes from 1 to 9, and the default value is 5.��.��M/Z :�� -'   $��	mzDecimal��$��75��$��Center=�& �� $&'  Pparse_advanced.Mz_decimal_index�.@��� .��1�� .��2�� .��3�� .��4�� .��5�� .��6�� .��7�� .��8�� .��9�� $��Center=�$��80,0,0,0q�$	��115��$s$ mThis option specifies the decimal place of intensity. Its value goes from 1 to 9, and the default value is 1.��.��Intensity :�� -(   $��IntensityDecimal��$��75��$��Center=�& �� $-' 'Pparse_advanced.Intensity_decimal_index�.@��� .��1�� .��2�� .��3�� .��4�� .��5�� .��6�� .��7�� .��8�� .��9�� $��0��$ 
Horizontal=�$��Center=�.+��� $��	10,10,0,0q�$	��110��$
��Left=�$��Center=�MS Data Format :�� -)   $��FormatChoices��$
��Left=�$��	15,10,0,0q�$	��120��$��Center=�& �� $' File_format_index�.@��� .��MGF�� .��RAW�� -*   $��IdentificationTab��$��Identification��% Z� .���� -+   $��IdentScroll��$
��Auto=�$
��Auto=�$��0,10,0,0q�.��� -,   $��gridIndentify�� �� $
��Auto ��� $
��Auto �.+�9� --   $��search��$��0��
 . - . ����   
LabelStyle#��[ $��Center=�.��Database Search.��� -.   $��
searchGrid�� �� $
��Auto ��� $
��Auto �.+�� $��0��$��0,5,0,0q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��0��$ 
Horizontal=�.+��� $��86��$��Center=�$
��Left=�.��
Database :�� -/   $��Database��$	��200��$��Center=�& �� $' Db_index��� -0   $��dbtip��$��10,0,0,0q�$��Center=�
���  ��  
DBtipStyle#��[ *�� $��1��$��0��$ 
Horizontal=�$��42��$��0,0,10,0q�.+��� $
��Left=�$��Center=�$��5,0,0,0q�Precursor Tolerance :�� $��Center=�$��Right=�$��10,0,0,0q�.��±�� -1   $��PreTol��$��80��$��Center=�$=$ 7Hint: The value of the tolerance cannot be less than 0.��.������ $. TwoWay=�$' Ptl.Tl_value�/     .���� -2   $��	ptolUnits��$& 0��{�.  $��55��$��Center=�$��
10,10,0,10q�.@��� .��Da�� .��ppm�� $��2��$��0��$ 
Horizontal=�$��42��$��5,0,0,0q�.+��� $
��Left=�$��Center=�Fragment Tolerance :�� $��Center=�$��Right=�$��10,0,0,0q�.��±�� -3   $��FraTol��$��80��$��Center=�$=$ 7Hint: The value of the tolerance cannot be less than 0.��.������ $' Ftl.Tl_value�$. TwoWay=�/     .���� -4   $��	ftolUnits��$& 1��{�.  $��60��$��Center=�$��10,0,0,0q�.@��� .��Da�� .��ppm�� $��3��$��0��$ 
Horizontal=�$��	0,10,0,12q�$��36��.+��� $	��142��$��Center=�$
��Left=�$G$ AMaximum number of variable modifications allowed on each protein.��.��Max Modify Position :%  Xceed.Wpf.Toolkit.IntegerUpDown  -5   $��maxmod��$��80��$��Center=�$G$ AMaximum number of variable modifications allowed on each protein.��,  &Xceed.Wpf.Toolkit.Primitives.InputBase1   Text1 �� $. TwoWay=�$' Max_mod�/     �� -6   $��	ModConfig��$��1��$��2��$��Add Modification��.��� $��0,10,0,0q� �� $
��Auto ��� $
��Auto ��� $��42 �2 � ColumnDefinitions2 �� $
��Auto ��� $
��Auto ��� $	��100 ��� $
��Auto �.+��� $��0��$��0��$��	10,0,15,0q�$
��Auto��$��Center=�Fixed�� -7   $��selectedFixMod��$��0��$��1��$	��180��$	��110��$��0,5,0,0q����	#eef6f6f63 �� SelectionMode$3 Extended=����� $' Fix_mods��� $��0��$��2��$��Center=�.+��� -8   $��40��$��25��$��10q�.���� $
��Fill=�$��images/icons/arr_left.png���� -9   $��40��$��25��$��10q�.���� $
��Fill=�$ ��images/icons/arr_right.png���� $��1��$��0��$��	10,0,15,0q�$
��Auto��$��Center=�
Variable�� -:   $��selectedVarMod��$��1��$��1��$	��180��$	��110��$��0,10,0,0q����	#eef6f6f6$3 Extended=����� $' Var_mods��� $��1��$��2��$��Center=�.+��� -;   $��40��$��25��$��10q�.���� $
��Fill=�$��images/icons/arr_left.png���� -<   $��40��$��25��$��10q�.���� $
��Fill=�$ ��images/icons/arr_right.png���� -=   $��Modifications��$��0��$��2��$��3��$	��280��$	��230��$3 Extended=����	#aaF6F6F6
�������$��0,5,0,0q����� �� $��2��$��3��$ 
Horizontal=�$��5q�$��Center=�.+��� ->   $��	disAllMod��$��25��$��Center=��� $��Center=�Display All�� -?   $��Center=�$��	100,0,0,0q�$��60��$��25��$��Italic$�
�������.��	Edit...9� -@   $��filter��$��1��
 . - . ���� #��[ $��Center=�.��Result Filter.���� $ 
Horizontal=�$��10q�.+��� $��FDR ≤ ��$��Center=��� -A   $��FDRValue��$��70��$��Center=�$7$ 1Hint: The value of FDR must be between 0 and 100.��.������ $' Filter.Fdr_value�$. TwoWay=�4 �� UpdateSourceTrigger$4 PropertyChanged=�/     .���� $
�� %  ��$
��Left=�$��50��$��Center=��� -B   $��
SummaryTab��$��Summary��% Z� .���� -C   $��	SumScroll��$
��Auto=�$
��Auto=�$��0,0,0,0q�.���  �� $��28 ��� $
��Auto �.+�c� -D   $��0��$��28��5 `� IsLocked5 . .@��� -E   $��sumSave��#��[ $ _� .��Save the configuration.���� $��images/icons/save.gif���� -F   $��sumRun��$��5,0,5,0q�#��[ $ _� .��Start to run.���� $��images/icons/run.png���� -G   $��exp_pdf��$ _� .��)'Export this configuration to a pdf file.���� $!��images/icons/pdf_export.png��$��18��$��18��� $��1�� �� $
��Auto ��� $��60 �.+�� -H   6 �Name$6 all_summary��$��0�� �� $
��Auto ��� $
��Auto �.+�9� -I   $��	FiRScroll��$��0��$��MS Data��- . .���� -J   $��
fileReport��$��20,0,0,0q�  SummaryListView#��[ ) �� .P��� $��Property��$	* 195��+ �� *	_property+�� $��Value��+ �� *_value+�� $��Check��$* 0��+ �� *_check+9� -K   $��SRScroll��$��1��$��Identification��- . .���� -L   $��searchReport��$��20,0,0,0q�#��[ ) �� .P��� $��Property��$	* 195��+ �� *	_property+�� $��Value��+ �� *_value+�� $��Check��$* 0��+ �� *_check+�� $��1��$ 
Horizontal=�$��Right=�$��0,0,20,0q�.+��� -M   $��saveBtn��$��80��$��30��$��0,0,20,0q�  ButtonStyle1#��[ .��Save�� -N   $��startBtn��$��80��$��30��$��0,0,20,0q�$��Right=�#��[ .��Start�� -O   $��stopBtn��$��80��$��30��$��Right=�#��[ .��Stop�� $��1��
�������$��Stretch=�$��2��7 �� ResizeBehavior$7 PreviousAndNext=��� -P   $��
outputInfo��$��2��
�������$
��Auto��$��Stretch=�
 . 
����������� $ 
Horizontal=�.+��� $��images/icons/output.png���� #��[ .��Output.���  �� $
��Auto ��� $��* �.+��� $��0��$��28��$ 
Horizontal=�.+��� -Q   $��saveReportBtn��$��85��$��26��$
��Left=�$��Italic$�.��Save Report�� -R   $��Output��$��1��
�������"8 �� VerticalScrollBarVisibility$
8 Auto=�$9 �� HorizontalScrollBarVisibility$9 Hidden=�: �� TextWrapping$
: Wrap=�; �� 
IsReadOnly; . $��Courier New)�
�������
���   �.���� -S   $��statusPanel�����#007acc$��3��$��0��$��3��$��Stretch=�$��25��
�������.@��� $ 
Horizontal=����#007acc.+��� -T   $��status1��Ready