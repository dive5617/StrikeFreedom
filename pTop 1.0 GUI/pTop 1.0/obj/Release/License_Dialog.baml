   M S B A M L   `   `   `  ����   pTop 1.2    pTop.License_Dialog   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  License Dialog��$	��300��$	��400�� � Icon$ images\ptop.png�� � WindowStartupLocation$ CenterScreen=� � 
ResizeMode$ NoResize=�.���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��0.4* ��� $
��0.6* �.+��� $��0��$��0��$��5q�
User Name:�� -    ��Name$ name_txt��$��0��$��1��$��5q��� $��1��$��0��$��5q�Institute/Company Name:�� -   $ company_txt��$��01��$��1��$��5q��� $��2��$��0��$��5q�
Country:�� -    ��Name$ country_txt��$��2��$��1��$��5q��� $��3��$��0��$��5q�Email Address:�� -   $ 	email_txt��$��3��$��1��$��5q��� $��4��$��0��$��5q�Activation Code:�� -   $ code_txt��$��4��$��1��$��5q�
������� �� 
IsReadOnly . �� -   $��5��$��0��$��5q�$��
Send Email��	 � ToolTip	 _� .���� send mail to ptop@ict.ac.cn�� -   $��5��$��1��$��5q�.��Copy to Clipboard�� $��6��$��0��$��2��$��5q� . 
 �� TextWrapping$

 Wrap=�$��15(�
���  ��.��=;The email will be sent to ptop@ict.ac.cn for manual review.