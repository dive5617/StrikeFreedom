   M S B A M L   `   `   `  ���� @  <pFind, Version=1.0.0.1, Culture=neutral, PublicKeyToken=nullclr-namespace:pFindpFind  W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   SplitButtonStyle  �� 
TargetType �� Property �� Value �� 
TargetType'   !System.Windows.VisualStateManager    VisualStateGroups%  System.Windows.VisualStateGroup   Transitions%  System.Windows.VisualTransition   GeneratedDuration	   To   States   System.Windows.VisualState	   
Storyboard
 �� TargetProperty �� 
TargetName9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value R� KeyTime  System.Windows.Visibility   Visible R� Value ��Name 
� Center 
� GradientOrigin 
� RadiusX 
� RadiusY � Color � Offset � ColumnDefinitions J� SnapsToDevicePixels ��Name  DropDownButtonStyle J� Effect-  'System.Windows.Media.Effects.BlurEffect �� CornerRadius �� Triggers P� Property P� Value  ButtonFocusVisual  �� StrokeDashArray  ButtonNormalBackground! �� EndPoint" �� 
StartPoint  ButtonNormalBorder# �� Color8  2System.Windows.Media.Animation.EasingColorKeyFrame$ �� KeyTime% �� Value& A�Name' � MappingMode( �� Stretch) J� RenderTransformOrigin* �� Angle  	Checkmark  UpArrow  	DownArrow  
RightArrow 	 ContextMenuStyle3  -System.Windows.Media.Effects.DropShadowEffect+   ShadowDepth,   
BlurRadius�� F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml     localclr-namespace:pFind  %  &	        &	 *    &	 x    &	     &	     7  7  7  &	 e    &	 �    &	 �    &	 �    &		     �� "  ��.���  � ��$ 0q���  � ��
 �������  � ��$ Center=���  � ��$ Center=���  � �� �� " ��.R�� $��0,0,-9.333,0q�   CommonStates��   $ 0:0:0.2D�  $ 0D�$ Pressed��.   
Normal��  	MouseOver��.	 �� .��U� $O
 I(Panel.Background).(GradientBrush.GradientStops)[0].(GradientStop.Offset)�$ border��.\�  $ 0��$
 0.19Q�U� $O
 I(Panel.Background).(GradientBrush.GradientStops)[2].(GradientStop.Offset)�$ border��.\�  $ 0��$ 0.529Q�U� $O
 I(Panel.Background).(GradientBrush.GradientStops)[3].(GradientStop.Offset)�$ border��.\�  $ 0��$ 0.809Q�U� $6
 0(Panel.Background).(RadialGradientBrush.RadiusY)�$ border��.\�  $ 0��$ 1.396Q�  Pressed��.	 �� .��T� $
 (UIElement.Visibility)�$ border1��..�i� $ 0��# Z   Disabled��.+��� $ border��$��1q����	#FFe5eef9$��2����
� $ 0.475,0.954&�$ 0.475,0.954&�$ 0.635Q�$ 0.817Q�.��� $ 	#FF5cacee��$ 0.032Q�� $ 	#FF87cefa��$ 0.365Q�� $ 	#FF63B8FF��$ 0.429Q�� $ 	#FF1E90FF��$ 0.647Q�� $ 	#FF0c55b9��$ 0.978Q�._�� $��2q� �� $��0.836* ��� $��0.164* �.+��� #��z��#��z����. # z #��z���� $ DropDownButton��$�� ��$��1��$��Stretch=�$��Stretch=�#���  �� $ border1�����	#efa1b7ea$��1q�$��2��$��2q�$y�	Collapsed=���Z�    �� $ 0�����	#FFe9ecef$��2��$��1q���Z�  P�  � {� .  . ���  � �� �#ADADAD��  � ��$
 12pt(��� .���  � �� �� .R��� $��2q� . #��� ��$��1��$	  1 2R��� $	! 0,1&�$	" 0,0&�.��� $ #F3F3F3��$ 0Q�� $ #EBEBEB��$	 0.5Q�� $ #DDDDDD��$	 0.5Q�� $ #CDCDCD��$ 1Q��� $# 	#FF707070���� "  ��.���  �@�FocusVisualStyle8   ��  � ��8  ��  � ��8  ��  � ��$ 1q���  � ��# � ����  � ��$ Center=���  � ��$ Center=���  � �� �� " ��.R�� $��20��$��20��   CommonStates��.   
Normal��  	MouseOver��.	 �� .��U� $0
 *(Shape.Fill).(RadialGradientBrush.RadiusY)�$ ellipse��.\�  $ 0��$ 1.139Q��� $,
 &(Shape.Stroke).(SolidColorBrush.Color)�$ ellipse��.g�  $$ 0��$% 	#FF006E1A���� $H
 B(Shape.Fill).(GradientBrush.GradientStops)[0].(GradientStop.Color)�$ ellipse��.g�  $$ 0��$% 	#FF05E23A���� $H
 B(Shape.Fill).(GradientBrush.GradientStops)[1].(GradientStop.Color)�$ ellipse��.g�  $$ 0��$% 	#FF007C1E��  Pressed��.	 �� .���� $,
 &(Shape.Stroke).(SolidColorBrush.Color)�$ ellipse��.g�  $$ 0��$% 	#FF006E1A���� $H
 B(Shape.Fill).(GradientBrush.GradientStops)[0].(GradientStop.Color)�$ ellipse��.g�  $$ 0��$% 	#FF05E23A���� $H
 B(Shape.Fill).(GradientBrush.GradientStops)[1].(GradientStop.Color)�$ ellipse��.g�  $$ 0��$% 	#FF007C1E��  Disabled��.+�A� $& ellipse�����	#FF1D1D1D$��20��$��20��$��1����
� $ 0.505,0.835&�$ 0.505,0.835&�$ 0.625Q�$ 0.625Q�.��� $ 	#FF8D8D8D��$ 0Q�� $ 	#FF1D1D1D��$ 1Q�A� $��12��$��12.002��$	��Top=�$��3,1,3,0q���Z� ���� $! 0.5,1&�$' RelativeToBoundingBox=�$" 0.5,0&�.��� $ 0.377Q�� $ 	#CAFFFFFF��F� [����� �� �7�g�7�@�� �$�B�@'���U>@��d�@�F�գ@ѐ� 8�m ��,M��@s8 	$
( Fill=�$��0��
�������$��Stretch=�$��Stretch=�$��6.625,6.625,5.875,5.875q�$) 0.5,0.5&���Z� z�V� .~��� �� �� $* 90Q�T�  P�  �@J�IsKeyboardFocused . P�  �@d�	IsChecked$
 trueY�P�  � {� .  . ���  � �� �#ADADAD�<8M 0,5.1 L 1.7,5.2 L 3.4,7.1 L 8,0.4 L 9.2,0 L 3.3,10.8 Z��M 0,4 L 3.5,0 L 7,4 Z��M 0,0 L 3.5,4 L 7,0 Z��M 0,0 L 4,3.5 L 0,7 Z��� "  ��.���  � �� �� " ��.R��� ���	#ff5DC5F9$��1q�$ 0��$��3q�   $+ 1Q�$, 2Q����� $! 0.5,1&�$" 0.5,0&�.��� $ 	#ff0c55b9��$ 0.836Q�� $ 	#ff5DC5F9��$ 0.387Q�� $ 	#ff5DC5F9��$ 0.387Q�._�� .+��� $��2q�$��35��$	��Top=���Z� ���� $! 0.5,1&�$" 0.5,0&�.��� $ 	#5DFFFFFF��$ 0Q�� $ Transparent��$ 1Q��� $��	5,10,5,10q�