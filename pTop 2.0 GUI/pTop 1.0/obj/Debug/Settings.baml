   M S B A M L   `   `   `  ����  pTop 2.0    pTop.SettingsWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Settings��5   	   $	��280��6   $	��698��6'    � WindowStartupLocation$ CenterScreen=�63   .��5      � ���	#aaEFEFF2 � RowDefinitions 5   
   �� 5      $��78 �6   �� 5      $��50 �6   �� 5	      $��* �6   �� 5
      $
��Auto �6   �� 5      $��* �6   .+�5   
   � $��0��6    � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $
��Auto �6'   �� 5      $��* �6'   .+�5      �� -   5        $��threadNumWarn��5   (   $��2��6   $��Center=�6=   $��Center=�6X   $��	10,24,0,0q�6u   
���  ��$
��Wrap=�6�   6�   �� 5      $��0��6   $��Center=�6$   $��Right=�6?   $��	20,28,6,0q�6[   $��Number of Threads: ��6n   $	��180��6�   �� 5      -   5         ��Name$ tbthreadnum��5   '   $��1��6   $��52��6<   $��25��6G    �� SelectedIndex$ 0��6S   
�������$��0,28,0,0q�6|   $��Center=�6�   56      57      �� 58   
   $��1��6    �� Orientation$ 
Horizontal=�6"   $	��Top=�6;   .+�59      �� $��Center=�6   $��Right=�6/   $��20,0,5,0q�6K   $#��Default Data Repository Path:��6]   $	��180��6�   �� 5:      -   5        $��tboutputpath��5:      $	��300��6*   $��25��66   
�������6�   �� 5;      -   5        $��65��5;      $��25��6    $��20,0,0,0q�6-   .��6[   	Browse...6f   5<      �� 5=   
   $��2��6   .+�5>      �� -   5        $��diskTip��5>      $��Center=�6'   $��23,0,5,0q�6B   
����  �$
��Wrap=�6f   6|   5?      �� 5@   
   $��3��6   $ 
Horizontal=�6"   $��Center=�6;   .+�5A      �� -   5        $��65��5A      $��25��6    $��0,0,18,0q�6,   $��OK��6V   �� 5B      $��65��6   $��25��6    $��Cancel��6,    �� IsCancel . 6=   5C      5D      5E      