   M S B A M L   `   `   `  ���� F  BColorPicker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:ColorPickerPluysColorPickerPluys  !    ColorPickerPluys.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     0localclr-namespace:ColorPickerPluys       � Title$  
MainWindow��$	��450��$	��500�� � WindowStartupLocation$ CenterScreen=� � WindowStyle$
 None=� � 
ResizeMode$ NoResize=� � AllowsTransparency .  J� SnapsToDevicePixels . .���  � RowDefinitions �� $��30 ��� .+�� $��1��$��2q� �� �� $��30 � � ColumnDefinitions �� �� $	��110��.+�� $��0�� �� �� $��30 �.+��� -   $��ColorSelectorSlider��$��30�� � Maximum$
 1530Q�	 � Minimum$	 0Q�$��1��   SliderCustomStyle#��[  
 �� Orientation$
 Vertical=� K� ZIndex$ 2��� -   $
��dfas��$��1������  �� EndPoint$ 0.5,1&� �� 
StartPoint$ 0.5,0&�.���  � Color$ 	#FFFF0000�� � Offset$ 0Q�� $ 	#FFFFFF00��$ 
0.16666666Q�� $ 	#FF00FF00��$ 
0.33333333Q�� $ 	#FF00FFFF��$	 0.5Q�� $ 	#FF0000FF��$ 
0.66666666Q�� $ 	#FFFF00FF��$ 
0.83333333Q�� $ 	#FFFF0000��$ 1Q��� 
�������$��0 0 1 1q�._�� -   $��	ColorBord��.+��� -   $	��xfd�� �� Orientation$ Vertical=��� -   
������ }�. .+�F� -   $��ColorSelector�� �� Right$ -5��
 �� Top$ -5��
�������
���   �$��1����� .��@�  @� Center$	 5,5&� @� RadiusX$ 5Q� @� RadiusY$ 5Q�@� $	 5,5&�$ 3Q�$ 3Q�� $��1�� �� �� .+��� -   $��colorMoment��$��0�� �� CornerRadius$ 5��$��Center=�$��Center=� � ToolTip �� $��当前颜色��._�� $��60��$��60������ *ColorCurrent+�� $��1��
�������$��0 1 0 1q�._��� $��Center=�.+�� $��0 2 0 0q� �� �� .+��� $��0��$��R��$��Center=�$��Center=��� $��1��$�� 0 0 0 1q�
������  �� TextAlignment$ Center=����� *ColorR+� $��0 2 0 0q� �� �� .+��� $��0��$��G��$��Center=�$��Center=��� $��1��$�� 0 0 0 1q�
������ $ Center=����� *ColorG+� $��0 2 0 0q� �� �� .+��� $��0��$��B��$��Center=�$��Center=��� $��1��$�� 0 0 0 1q�
������ $ Center=����� *ColorB+� $��0 2 0 0q� �� �� .+��� $��0��$��A��$��Center=�$��Center=��� $ Vertical=�$��1��.+��� -   $ 1Q�$	 0Q� � Value �� *ColorA+�� $
��100%��$�� 0 0 0 1q�
������ $ Center=�� $��2��$��2�� �� �� �� $��30 ��� $	��110 �.+�� -	   $��OriginColor��$��0������ *ColorOrigin+ �� $��初始颜色��� -
   $��	LastColor��$��1������ *Tag.Background+ �� ElementName$ ColorPop�� �� $��上一种颜色���� -     GeneralButton#��[ $��2��
������� �� $��	取色器���� $��3��$��	#FFFFFFFF��$��Center=�$��Center=�$��0 0 0 1q�
������ 