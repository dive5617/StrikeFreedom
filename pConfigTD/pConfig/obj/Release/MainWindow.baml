   M S B A M L   `   `   `  ���� B  >pConfig, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:pConfigpConfig      pConfig.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     'localclr-namespace:pConfig     -     � Title$  pConfig��$	��614��$	��813�� � 
ResizeMode$ CanMinimize=� � Icon$ pConfig.ico�� J� CommandBindings )  #System.Windows.Input.CommandBinding  -      Command$! local:CustomCommands.DoSave�� J� InputBindings �� 
 �� Key$ S�� �� 	Modifiers$ Controlf� �� Command$! local:CustomCommands.DoSave��	 � 	Resources	 *  $System.Windows.Controls.DataGridCell
 �� 
TargetType �� Triggers P� Property P� Value �� Property �� Value ^� 
Conditions �� Binding �� RelativeSource �� Value �� AncestorType��%�  '        �� "
   P�  �@ 
IsSelected . . ���  � ��# � ����  � ��# � ����  � ��# � ��^�  ��  �� *
IsSelected+ �� *Self+ True��  �� *	IsFocused+ �� *Self+	 False.0���  � ��# � ����  � ��# � ����  � ��# � ��^�  ��  �� *
IsSelected+ �� *Self+ True��  �� *ContextMenu.IsOpen+ �� $ DataGridL� True.0���  � ��# � ����  � ��# � ����  � ��# � ��.���  � RowDefinitions �� $
��Auto ��� $��* �.+�o� $��0��.@�m� $
��Tool��.@�m� -   $"��Restore the default Settings��m� -   $��	N's Count��� $��1��.+��� -    ��Name$ tab_control��$
��Left=�$	��552��$��	10,10,0,0q�$	��Top=�$	��774��.@���  J� RenderTransformOrigin$ -0.279,0.609&�����  �� Orientation$ 
Horizontal=�.+��� $y�	Collapsed=��� $��	Databases��$��0,0,0,0q�.��� ���	#FFE5E5E5.+�&   System.Windows.Controls.DataGrid  -     Name$ db_listView��   HeadersVisibility$ Column=�   HorizontalGridLinesBrush
 ����    VerticalGridLinesBrush
 ���� $
��Left=�$	��448��$��	10,10,0,0q�$	��Top=�$	��744��   SelectionMode$ Extended=�   AutoGenerateColumns .     
IsReadOnly . ���� *	databases+    Columns  0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn!   Header$
! Name��1  +System.Windows.Controls.DataGridBoundColumn"   Binding" �� *	DB_Name+  $
! Path��" �� *	DB_Path+�� -   $	��Add��$
��Left=�$��112,481,0,0q�$	��Top=�$��75���� -   $��Delete��$
��Left=�$��342,481,0,0q�$	��Top=�$��75���� -	   $
��Save��$
��Left=�$��584,481,0,0q�$	��Top=�$��75���� ���� $ 
Horizontal=�.+��� $y�	Collapsed=��� $��Modifications��$��0,0,0,0q�.��� ���	#FFE5E5E5# � ColumnDefinitions# �� $
��226* ��� $
��539* �.+�  -
   $ mod_listView��$ Column=�
 ���� 
 ���� $
��Left=�$	��413��$��	10,45,0,0q�$	��Top=�$	��744��$��2��$ Extended=� .   . ���� *modifications+    $
! Name��" �� *Name+  $
! Mass��$   Width5  /System.Windows.Controls.DataGridLengthConverter$$ 50 " �� *
Mod_mass+  $! Composition��" �� *Composition+  $! Position��" �� *Position_Display+  $! Sites��$$ 50 " �� *
Mod_site+  $! Neutral_Loss��" �� *Neutral_loss_str+  $! Common��" �� *	Is_common+�� -   $	��Add��$
��Left=�$��112,481,0,0q�$	��Top=�$��75���� -   $��Delete��$
��Left=�$��116.018,481,0,0q�$	��Top=�$��75��$��1���� -   $
��Save��$
��Left=�$��358.018,481,0,0q�$	��Top=�$��75��$��1���� -   $��2��$
��Left=�$��22��$��	10,10,0,0q�$��	Search...��$	��Top=�$	��744��
��������� ���� $ 
Horizontal=�.+��� $y�	Collapsed=��� $��Quantifications��$��0,0,0,0q�.��� ���	#FFE5E5E5.+�  -   $ quant_listView��$ Column=�
 ���� 
 ���� $
��Left=�$	��448��$��	10,10,0,0q�$	��Top=�$	��744��$��3��$ Extended=� .   . ���� *quantifications+    $
! Name��" �� *Name+  $! Value��" �� *All_quant_str+�� -   $	��Add��$
��Left=�$��112,481,0,0q�$	��Top=�$��75���� -   $��Delete��$
��Left=�$��342,481,0,0q�$	��Top=�$��75���� -   $
��Save��$
��Left=�$��584,481,0,0q�$	��Top=�$��75���� $ -0.279,0.609&�$y�	Collapsed=����� $ 
Horizontal=�.+��� $y�	Collapsed=��� $��Enzymes��$��0,0,0,0q�.��� ���	#FFE5E5E5# �� $
��433* ��� $
��331* �.+�  -   $ enzyme_listView��$ Column=�
 ���� 
 ���� $
��Left=�$	��448��$��	10,10,0,0q�$	��Top=�$	��744��$��2��$ Extended=� .   . ���� *	enzymes+    $
! Name��" �� *Name+  $! Cleave��" �� *Cleave_site+  $! N/C Term��" �� *N_C+  $! Ignore��" �� *Ignore_site+�� -   $	��Add��$
��Left=�$��112,481,0,0q�$	��Top=�$��75���� -   $��Delete��$
��Left=�$��342,481,0,0q�$	��Top=�$��75���� -   $
��Save��$
��Left=�$��151,481,0,0q�$	��Top=�$��75��$��1���� $ -0.279,0.609&����� $ 
Horizontal=�.+��� $y�	Collapsed=��� $��Amino Acids��$��0,0,0,0q�.��� ���	#FFE5E5E5.+�  -   $ aa_listView��$ Column=�
 ���� 
 ���� $
��Left=�$	��448��$��	10,10,0,0q�$	��Top=�$	��744��$ Extended=� .   . ���� *aas+    $
! Name��" �� *Name+  $! Composition��" �� *Composition+  $
! Mass��" �� *Mass+�� -   $
��Edit��$
��Left=�$��112,481,0,0q�$	��Top=�$��75���� -   $
��Save��$
��Left=�$��584,481,0,0q�$	��Top=�$��75���� $ -0.279,0.609&����� $ 
Horizontal=�.+��� $y�	Collapsed=��� $��Elements��$��0,0,0,0q�.��� ���	#FFE5E5E5.+�  -   $ element_listView��$ Column=�
 ���� 
 ���� $
��Left=�$	��413��$��	10,45,0,0q�$	��Top=�$	��744��$ Extended=� .   . $��4������ *
elements+    $
! Name��" �� *Name+  $
! Mass��" �� *All_Mass_Str+�� -   $	��Add��$
��Left=�$��112,481,0,0q�$	��Top=�$��75���� -   $��Delete��$
��Left=�$��342,481,0,0q�$	��Top=�$��75���� -   $
��Save��$
��Left=�$��584,481,0,0q�$	��Top=�$��75���� -   $��4��$
��Left=�$��22��$��	10,10,0,0q�$��	Search...��$	��Top=�$	��744��
�������