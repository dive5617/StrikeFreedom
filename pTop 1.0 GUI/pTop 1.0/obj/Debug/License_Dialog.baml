   M S B A M L   `   `   `  ����  pTop 1.2    pTop.License_Dialog   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  License Dialog��5   	   $	��300��6    $	��400��6-    � Icon$ images\ptop.png��69    � WindowStartupLocation$ CenterScreen=�6P    � 
ResizeMode$ NoResize=�6u   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5	      $
��Auto �6   6,   �� 5
      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,    � ColumnDefinitions 5   
   �� 5      $
��0.4* �6   6.   �� 5      $
��0.6* �6   6.   .+�5   
   �� $��0��6   $��0��6!   $��5q�61   
User Name:6H   �� 5   
   -   5         ��Name$ name_txt��5   /   $��0��6   $��1��6   $��5q�6A   6N   �� 5   
   $��1��6   $��0��6!   $��5q�61   Institute/Company Name:6U   �� 5   
   -   5        $ company_txt��5   0   $��01��6   $��1��6    $��5q�6E   6R   �� 5   
   $��2��6   $��0��6!   $��5q�61   
Country:6F   �� 5   
   -   5         ��Name$ country_txt��5   0   $��2��6   $��1��6    $��5q�6E   6R   �� 5   
   $��3��6   $��0��6!   $��5q�61   Email Address:6L   �� 5   
   -   5        $ 	email_txt��5   /   $��3��6   $��1��6   $��5q�6B   6O   �� 5   
   $��4��6   $��0��6!   $��5q�61   Activation Code:6N   �� 5   
   -   5        $ code_txt��5   /   $��4��6   $��1��6   $��5q�6A   
������� �� 
IsReadOnly . 6r   �� 5   
   -   5        $��5��5      $��0��6   $��5q�6.   $��
Send Email��6O   	 � ToolTip	 5      _� 5      .��5       �� send mail to ptop@ict.ac.cn6=   5!      5#      �� 5$   
   -   5        $��5��5$      $��1��6   $��5q�6.   .��6O   Copy to Clipboard6b   �� 5%   
   $��6��6   $��0��6   $��2��6/   $��5q�6C    . 
 �� TextWrapping$

 Wrap=�6`   $��15(�5&      
���  ��.��61   =;The email will be sent to ptop@ict.ac.cn for manual review.6n   5'      5(      