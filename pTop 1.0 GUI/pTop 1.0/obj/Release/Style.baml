   M S B A M L   `   `   `  ���� C  ?pTop 1.2, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:pToppTop  W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   ListViewSelectedBrush  �� EndPoint �� 
StartPoint � Color � Offset  ListViewItemContainerStyle �� 
TargetType �� Property �� Value �� 
TargetType ��Name	 J� SnapsToDevicePixels
 �� Triggers P� Property P� Value �� 
TargetName ]� 
Conditions �� Property �� Value  open_folder_cm  
LabelStyle  DFVisibility �� Triggers �� Binding �� ElementName �� Path �� Value  
DBtipStyle  ButtonStyle1'   !System.Windows.VisualStateManager    VisualStateGroups%  System.Windows.VisualStateGroup   Name   States   System.Windows.VisualState   Name   
Storyboard �� 
TargetName �� TargetProperty	 W� To g� Duration	 �� To  J� Opacity! �� Stretch" 
� GradientOrigin# 
� RadiusX$ 
� RadiusY% �� TextAlignment  ButtonOpacity  SummaryListView& �� 	Resources 	 Key 
 	LossBrush  ProfitBrush  InvalidBrush  PointlessBrush�� F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml     localclr-namespace:pTop  %�  &	        &	 \     7   7   &	 �    &	 @    &	 �    '_� '    &	 �    &	 !
    &	 �
    7 
 7  7  7  �� $	  0,1&�$	 0,0&���� � $ 	#ff75aac7��$ 0Q�� $ 	#aa3399ff��$ 1Q��� " ��.���  � �� �	#ffffffff��  � ��$
 Left=���  � ��$ Center=���  � ��
 �   ���  � ��$ 0,0,0,0q���  � �� �� " ��.R��� $ Bd��#��z��	 . $��0,0,0,1q����#6FBDE8._��� #��z��
 P�  �@��
IsSelected . . ��� $ Bd�� � �� �	#993399ff�� $ Bd��8    � ��P�  �@J�IsMouseOver . . ��� $ Bd�� � �� �#e0eff8]�  ��  �@��
IsSelected . ��  �@��IsSelectionActive . ./��� $ Bd��8   � ���� $ Bd�� � �� �	#FF143c65��  � ��
 �������  �@�ContextMenu# �  �� " ��.���  � ��$ 15(���  � �� �	#ee100000��  � ��$ Normal$���  � ��$
 Bold"��� " �� ��  �� $ fileList��$ Items.Count� 0.a���  � y�$ 	Collapsed=��� " ��.���  � y�$ 	Collapsed=� ��  �� $ Database��$ SelectedIndex� -1.a���  � y�$ Visible=��� " _�.���  �@_�HorizontalOffset$ 0����  �@_�VerticalOffset$ 0����  � �� �	#ffE9ECEF��  � ��
 �   ���  � ��$ 12(��� " ��.���  � �� �� " ��.R�� 	 .    $ CommonStates��.   $ Normal��. �� .��W� $ buttonShape��$ Opacity�$ 1X�$ 0D�  $ Pressed��. �� .��W� $ buttonShape��$ Opacity�$ 1X�$ 0:0:0.1D�W� $ buttonShape��$ Opacity�$ 0X�$ 0D��� $ 
buttonText��$4 .(TextBlock.Foreground).(SolidColorBrush.Color)�$ BlackX�$ 0:0:0.1D�  	MouseOver��. �� .��W� $ buttonShape��$ Opacity�$ 1X�$ 0:0:0.1D�W� $ buttonShape��$ Opacity�$ 1X�$ 0:0:0.1D�  Disabled��. �� .��W� $ buttonShape��$ Opacity�$	 0.5X�$ 0:0:0.1D�  $ FocusStates��.   $ Focused��. ��   $ 	Unfocused��. �� .+��� $��
hoverShape��$��30��$��0,0,1,0q�	 . $��0,0,1,0q�._��� ���	#ffa1b7ea$	��0,1q����	#ffe5eef9$��40��	 . �� $��buttonShape��$  0Q����	#ffe9ecef$��0q�$��30��	 . ._��� $
! Fill=�$��30����
� $" 0.2,0.5&�$	# 0.8Q�$	$ 0.8Q�.��� $ 	#ff5fa3f6��$ 0Q�� $ 	#ff0c55b9��$ 1Q��� $��NormalShape��$  0Q����	#ffe9ecef$��0q�$��30��	 . ._��� $
! Fill=�$��30����
� $" 0.2,0.5&�$	# 0.8Q�$	$ 0.8Q�.��� $ 	#ff5fa3f6��$ 0Q�� $ 	#ff5DC5F9��$ 1Q��� $��
buttonText��$
��12pt(�
�������#��z��$% Justify=�$��Center=�$��Center=��� " �� P�  � {� .  . ���  �@J�Opacity$	 0.3Q�P�  � {� . . ���  �@J�Opacity$ 1Q��� " ��& ���� ProfitBrush	 $ 0.5,0&�$  0.5,1&�.��� $	 0.1Q�$ 	#AA00CC00��� $	 0.8Q�$ 	#66009900���� 	LossBrush	 $ 0.5,0&�$  0.5,1&�.��� $	 0.1Q�$ 	#AACd0000��� $	 0.8Q�$ 	#55890000���� InvalidBrush	 $ 0.5,0&�$  0.5,1&�.��� $	 0.1Q�$ 	#fff7b883��� $	 0.8Q�$ 	#55880000���� PointlessBrush	 $ 0.5,0&�$  0.5,1&�.��� $	 0.1Q�$ 	#AA63d1ac��� $	 0.8Q�$ 	#55008800��.���  � ��$ Stretch=� ��  �� $ _value� null.a���  � ��8   ��  �� $ _value�  .a���  � ��8  ��  �� $ _check� invalid.a���  � ��8  ��  �� $ _check� 	pointless.a���  � ��8  