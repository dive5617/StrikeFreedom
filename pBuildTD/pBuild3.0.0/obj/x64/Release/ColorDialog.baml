   M S B A M L   `   `   `  ���� A  =pBuild, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:pBuildpBuild      pBuild.ColorDialog   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     &localclr-namespace:pBuild       � Icon$  
pBuild.ico�� � Title$ 
Pick Color��$	��410��$	��520�� � WindowStartupLocation$ CenterOwner=� � ShowInTaskbar .   � 
ResizeMode$ CanResizeWithGrip=� � ShowActivated .  $	��410��$	��520��.���  � RowDefinitions �� $��* ��� $��40 �.+�   pBuild.ColorPicker  -     Name$ colorPicker��$��
0,2,-12,30q�$��0��$��2��$��2���� $��1��$��Right=� �� Orientation$ 
Horizontal=�$��5q�.+��� -   	 ��Name$	 btnOK��$��OK��
 �� 	IsDefault
 . $��60��$��25��$��10,0,0,0q�$��0���� -   $	 	btnCancel��$��Cancel�� �� IsCancel . $��25��$��60��$��	10,0,10,0q�$��1��