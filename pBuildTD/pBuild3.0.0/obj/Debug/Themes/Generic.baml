   M S B A M L   `   `   `  ����A  =pBuild, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:pBuildpBuild  5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"    pBuild.TreeListViewConverter   TreeListViewConverter  �� 
TargetType  TreeListViewItem ��Name �� Content �� Columns �� RelativeSource �� Mode �� AncestorType ��Name �� Triggers	 P� Property
 P� Value �� 
TargetName �� Property �� Value ]� 
Conditions �� Property �� Value!   pBuild.TreeListViewExpander  TreeListViewExpander ��Name �� Orientation �� Self �� 	Converter d� 	IsChecked �� Value �� Binding   pBuild.TreeListView �� 
TargetType � 	Resources � RowDefinitions   Columns   AllowsColumnReorder �� AllowsColumnReorder�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      localclr-namespace:pBuild  5      %Y  &	        &	      &	 r    7   '  �    7  7     5      6"   �� 5
      "  Q�6   .R�5   
   �� .+�5      �� $ Border��6   ._�5      �� # z��6'    5      �� *	Columns+ �� $ FindAncestor=�$ local:TreeListViewL�5      �� 5      $ ItemsPresenter��6   $y�	Collapsed=�65   5       5   
   P� 5      	 �@Q�
IsExpanded
 . . �5      �� $ ItemsPresenter��6    � y�$ Visible=�6K   5      P� 5      	 �@Q�
IsSelected
 . . �5       �� $ Border��6   # � ��5!       � ���� 5#      # � ��5$       � ��5%      ]� 5'       5(      �� 5)       �@Q�
IsSelected . 66   �� 5*       �@Q�IsSelectionActive .  6=   ./�5-      �� $ Border��6   # � ��5.       � ���� 50      # � ��51       � ��52      P� 54      	 � {�
 .  . �56      �� # � ��57       � ��58      5:      �� 5=      "   6   .R�5>   
   �� $ ContainerElement��6.   $ 
Horizontal=�6   .+�5@      � ��6   �� # Z 8   d� 5C      $��9��5D   .   $��9��68   $��Center=�6C   $��1q�6^   
�����_� 5C      �� *
IsExpanded+ �� $ FindAncestor=�$ TreeViewItemL�5D   i   5E       5F   
   �� 5H      $ False��5I   .    5H      �� *
HasItems+ �� $ FindAncestor=�$ TreeViewItemL�.a�5J      �� $ ContainerElement��6    � y�$ Hidden=�6M   5K      5M      �� 5P      "  6   .�5R   
   ��  � �� 5S      �� 5T      "   6"    5U      ���� 5W      'Q�       " Q�6    .�5X      ��  � ��8   69   5Y      �� 5[      '        "  6    .�5\      ��  � ��8  69   5]      5^      .R�5`      �� #��z��6   #��z��5a      #��z��5b      ._�5e      �� $
��Auto=�6'   $��Disabled=�5f   '   .��5h      �  5i   "   �� 5k   &   $
��Auto �64   �� 5m   &   $��* �64   .+�5p   "   �� # z 6=   # z 5q   %   �� 5t   "   $��Disabled=�6/   $
��Auto=�5u   /   $��1��5v   /   .��5x   &   �� 5y   #   5z      5{      5|      5}      5      5�      5�      