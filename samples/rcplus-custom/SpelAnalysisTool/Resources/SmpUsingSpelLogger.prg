#include "AdjParams.inc"

Function SmpUsingSpelLoggerForSCARA
	Integer i
	Reset
	Motor On
	Power Low
	Speed 10
	Accel 10, 10
	Go JA(90, 0, 0, 0)
	
	SpelLog_Init(True, 0.05, "RuntimeLog", False)
	
	For i = 1 To 3
		SpelLog_CycleStart
		
		SpelLog_SectionStart("Go Right")
		SpelLog_MotionStart
		Go JA(90, -30, 0, 0)
		SpelLog_MotionEnd
		SpelLog_SectionEnd("Go Right")
		
		SpelLog_SectionStart("Go Left")
		SpelLog_MotionStart
		Go JA(90, 30, 0, 0)
		SpelLog_MotionEnd
		SpelLog_SectionEnd("Go Left")
		
		SpelLog_SectionStart("Go Home")
		SpelLog_MotionStart
		Go JA(90, 0, 0, 0)
		SpelLog_MotionEnd
		SpelLog_SectionEnd("Go Home")
		
		SpelLog_CycleEnd
	Next
	
	SpelLog_Fin
Fend

Function SmpUsingSpelLoggerForSixAxes
	Integer i
	Reset
	Motor On
	Power Low
	Speed 10
	Accel 10, 10
	Go JA(0, 0, 0, 0, 0, 0)
	
	SpelLog_Init(True, 0.05, "RuntimeLog", False)
	
	For i = 1 To 3
		SpelLog_CycleStart
		
		SpelLog_SectionStart("Go Downward")
		SpelLog_MotionStart
		Go JA(0, 0, 0, 0, -90, 0)
		SpelLog_MotionEnd
		SpelLog_SectionEnd("Go Downward")
		
		SpelLog_SectionStart("Go Home")
		SpelLog_MotionStart
		Go JA(0, 0, 0, 0, 0, 0)
		SpelLog_MotionEnd
		SpelLog_SectionEnd("Go Home")
		
		SpelLog_CycleEnd
	Next
	
	SpelLog_Fin
Fend
