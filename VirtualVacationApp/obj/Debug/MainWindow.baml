   M S B A M L   `   `   `  ����S  OMicrosoft.Kinect.Toolkit, Version=1.8.0.0, Culture=neutral, PublicKeyToken=nulleHclr-namespace:Microsoft.Kinect.Toolkit;assembly=Microsoft.Kinect.ToolkitMicrosoft.Kinect.Toolkit  5   	   K GDepthMergeEffect, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullM8clr-namespace:DepthMergeEffect;assembly=DepthMergeEffectDepthMergeEffect 5   	    VirtualVacation9   3Microsoft.Samples.Kinect.VirtualVacation.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   VkHclr-namespace:Microsoft.Kinect.Toolkit;assembly=Microsoft.Kinect.Toolkit     5   	   Hdme8clr-namespace:DepthMergeEffect;assembly=DepthMergeEffect     5   	   -   5          � Title$  Virtual Vacation��5   	   $
��1000��6"   $
��1920��60    � 	Resources 5         liveDepthMap �� RelativeTransform �� CenterX �� CenterY  liveColorMap  backgroundDepthMap  MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value  SnapshotButton	 �� 
TargetType
 �� Orientation ��Name �� TextAlignment ��Name �� Triggers P� Property P� Value �� 
TargetName  SquareCheckBox �Name��%�  &	        &	 P     &	 �     &	 �     &	     &	 6    '�� Y    &	 �    7  7  &	 �    7  7  �� 5	   
    5
      �� 5      $	 0.5Q�6"   $	 0.5Q�60   5      �� 5   
    5      �� 5      $	 0.5Q�6"   $	 0.5Q�60   5      �� 5   
    5      �� 5      $	 0.5Q�6"   $	 0.5Q�60   5      �� 5   
   $ 	#ff6e6e6e��62   �� 5   
   $ 	#ff52318f��64   �� 5   
   $ 	#ff00BCF2��62   �� 5   
   " ��6   .�5      ��  �@J�SnapsToDevicePixels . 64   5      �� 5   
   " ��6   .�5      ��  � �� 5       �� 5!      "	 ��6&   .R�5"      � .+�5#      �� $
 
Horizontal=�6)   
������ .+�5$   "   �� $ SnapText��6,   #��z��6>   $
 Left=�6_   $��Center=�6u   8��  $��15(�6�   � 5%   "   $��9,0,0,0q�6'   .+�5&   &   �� $ 
SnapNormal��6,   $��Images\SnapNormal.png��6@   $
��None=�6_   $��Center=�6n   �� 5'   &   $ 	SnapHover��6,   $��Images\SnapHover.png��6?   $
��None=�6]   $��Center=�6l   $y�	Collapsed=�6�   5(   #   5)      5*       5+      P� 5,       �@J�IsMouseOver . . �5-   "   �� $ 
SnapNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5.   "   $ 	SnapHover��6O    � y�$ Visible=�6?   6O   �� 5/   "   8  $ SnapText��6h    � ��50      52      54      55      �� 57   
   " ��6   .�58      ��  � �� 59      �� 5:      "	 ��6&   .R�5;      � .+�5<      �� $
 
Horizontal=�6)   
������ .+�5=   "   � $ SquareCheckBoxChecked��6'   .+�5>   &   �� $ CheckedNormal��6,   $��Images\CheckedNormal.png��6C   $
��None=�6e   $��Center=�6t   �� 5?   &   $ CheckedHover��6,   $��Images\CheckedHover.png��6B   $
��None=�6c   $��Center=�6r   $y�	Collapsed=�6�   5@   #   � 5A   "   $ SquareCheckBoxUnchecked��6'   $y�	Collapsed=�6H   .+�5B   &   �� $ UncheckedNormal��6,   $ ��Images\UncheckedNormal.png��6E   $
��None=�6i   $��Center=�6x   �� 5C   &   $ UncheckedHover��6,   $��Images\UncheckedHover.png��6D   $
��None=�6g   $��Center=�6v   $y�	Collapsed=�6�   5D   #   �� 5E   "   $ SquareCheckBoxText��6,   #��z��6H   $
 Left=�6i   $��Center=�6   8��  $��15(�6�   $��9,0,0,0q�6�   5F      5G       5H      P� 5I       �@d�	IsChecked$ falseY�6;   . �5J   "   �� $ SquareCheckBoxChecked��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5K   "   $ SquareCheckBoxUnchecked��6O    � y�$ Visible=�6?   6O   5L      P� 5M       �@J�IsMouseOver . . �5N   "   �� $ CheckedNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5O   "   $ UncheckedNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5P   "   $ CheckedHover��6O    � y�$ Visible=�6?   6O   �� 5Q   "   $ UncheckedHover��6O    � y�$ Visible=�6?   6O   �� 5R   "   8  $ SquareCheckBoxText��6h    � ��5S      5U      5W      5X      5Y      .��5[      � -   5        $��
layoutGrid��5[      $��	10 0 10 0q�6    � RowDefinitions 5\   
   �� 5]      $
��Auto �6   �� 5^      $��* �6   �� 5_      $
��Auto �6   �� 5`      $
��Auto �6   �� 5a      $
��Auto �6   .+�5c   
   � $��0��6   $��0 0 0 20q�6    � ColumnDefinitions 5d      �� 5e      $��* �6#   �� 5f      $��* �6#   �� 5g      $��* �6#   .+�5i      �� $��0��6   $��Images\Logo.png��6$   $
��Fill=�6=   $��32��6L   $��81��6X   $
��Left=�6c   $��0 10 0 5q�6~   �� 5j      $��2��6   $��0 0 -1 0q�6(   $��Right=�6:   $��Bottom=�6V   #��[ 6q   $��Segoe UI)�6�   $��18(�6�   Virtual Vacation6�   4   .Microsoft.Kinect.Toolkit.KinectSensorChooserUI  5k      -   5        $��sensorChooserUi��5k   |   $��1��6&   $��Center=�66   $��0 0 0 5q�6S   $	��Top=�6d   6|   5l      � 5m   
   $��1��6   .+�5n      �� -   5        $��Backdrop��5n      $��Images/Background.jpg��6$   $
��None=�6C    J� Effect 5o      !  DepthMergeEffect.DepthMerge  5p         MaskedActor# [ 6%      
ActorDepth# [  6Q      BackgroundDepth# [ 6|      
ActorScale 5q   $   ��  �� ElementName$ actorScaleSlider�� �� Path$ Value�   ActorXOffset 5r   $   �� $ actorXOffsetSlider��$ Value�   ActorYOffset 5s   $   �� $ actorYOffsetSlider��$ Value�5u      5v      � 5w   
   $��2��6   .+�5x      �� .+�5z      �� $
 
Horizontal=�6   .+�5{      �� .��6   
ActorScale6$   �� 5|      -   5        $��actorScaleSlider��5|   �   $��25��6   $
��Left=�6%    � Minimum$ 0Q�6@    � Maximum$ 10Q�6L     � SmallChange$  .1Q�6Y   ! � LargeChange$! 1Q�6j   " � Value$" 1Q�6z   $��Bottom=�6�   $	��289��6�   �� 5}      $��28��6   $
��Left=�6$   $��Bottom=�6?   $	��120��6Z   ��6f   �� $ actorScaleSlider��$ Value�5~      �� 5�      $
 
Horizontal=�6   .+�5�      �� .��6   Actor X Offset6(   �� 5�      -   5        $��actorXOffsetSlider��5�   �   $��25��6   $
��Left=�6)   $
 -5.0Q�6D   $	 5.0Q�6S   $
  0.05Q�6a   $
! 0.05Q�6t   $	" 0.0Q�6�   $��Bottom=�6�   $	��289��6�   �� 5�      $��28��6   $
��Left=�6(   $��Bottom=�6C   $	��120��6^   ��6j   �� $ actorXOffsetSlider��$ Value�5�      �� 5�      $
 
Horizontal=�6   .+�5�      �� .��6   Actor Y Offset6(   �� 5�      -   5        $��actorYOffsetSlider��5�   �   $��25��6   $
��Left=�6%   $
 -5.0Q�6@   $	 5.0Q�6O   $
  0.05Q�6]   $
! 0.05Q�6p   $	" 0.0Q�6�   $��Bottom=�6�   $	��289��6�   �� 5�      $��28��6   $
��Left=�6$   $��Bottom=�6?   $	��120��6Z   ��6f   �� $ actorYOffsetSlider��$ Value�5�      5�      5�      �� 5�   
   -   5        $��buttonScreenshot��5�   �   $��3��6   #��[ 6   $��
Screenshot��6F   $
��Auto��6[   $��Right=�6i   $��Center=�6�   $��
10 10 0 10q�6�   6�   �� 5�   
   -	   5        $��checkBoxNearMode��5�   �   $��3��6   #��[ 6    $��	Near Mode��6H   $
��Auto��6\   $
��Left=�6j   $��Center=�6�   $��
0 10 10 10q�6�   6�   �� 5�   
   -
   5        $��	statusBar��5�   ?   $��4��6   $��Stretch=�6!   $��Bottom=�6P   
�������#��[ 6~   .@�5�      �� $��0 0 0 10q�6   .��5�      �� -   5        $��statusBarText��5�      $��-1 0 0 0q�61   JHPress 'Screenshot' to save a screenshot to your 'My Pictures' directory.6�   5�      5�      5�      5�      