   M S B A M L   `   `   `  ����  pTop 2.0    pTop.NewTask   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  NewTask��5   	   $	��240��6   $	��540��6&    � Icon$ images/pTop.ico��5   	   .��5      � ���	#aaEFEFF2 � RowDefinitions 5   
   �� 5      $��80 �6   �� 5	      $��50 �6   �� 5
      $
��Auto �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   .+�5      �� $��Center=�6   $��Right=�6/   $��	20,0,18,0q�6K   .��6^   Name6d   �� 5      -   5        $��newTaskName��5      $	��300��6)   $��25��65   
�������6Z   5      �� 5   
   $��1��6   $ 
Horizontal=�6"   $	��Top=�6;   .+�5      �� $��Center=�6   $��Right=�6/   $��20,0,5,0q�6K   .��6]   
Location6g   �� 5      -   5        $��newTaskPath��5      $	��300��6)   $��25��65   
�������6Z   �� 5      -   5        $��65��5      $��25��6    $��20,0,0,0q�6-   .��6S   	Browse...6^   5      �� 5   
   $��2��6   $ 
Horizontal=�6"   .+�5      �� -   5        $��newTaskWarning��5      $	��240��6*   $��40,0,0,0q�66   $��Center=�6H   6g   �� 5      -   5        $��65��5      $��25��6    $��	40,0,18,0q�6,   .��6T   OK6X   �� 5      -   5        $��65��5      $��25��6    .��6D   Cancel6L   5      5      5      