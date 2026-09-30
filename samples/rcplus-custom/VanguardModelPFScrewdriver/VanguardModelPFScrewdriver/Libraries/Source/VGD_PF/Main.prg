#include "VGD_PF_defines.inc"
 ' Function main
Function Lib2692473628_F1
	Lib2692473628_F2 ("Driver")
Fend
 ' Function Test1 (defName$ As String)
Function Lib2692473628_F2 (defName$ As String)
	
	Integer ret
	Integer cid
	
	Lib2692473628_G1 = True
	
	 ' Connect
	ret = VGD_PF_Connect (defName$, ByRef cid)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "Connected cid = ", cid
	Lib2692473628_F3 (cid)
	
	 ' Disconnect
	ret = VGD_PF_Disconnect (cid)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "Disconnected"
Fend
 ' Function Test1Inner (cid As Integer)
Function Lib2692473628_F3 (cid As Integer)
	Integer ret
	String vacuumOn$, vacuumBreakOn$, suctionState$
	UShort programNo
	Integer count, seconds
	
	 ' Get I/O Labels
	ret = VGD_PF_GetVacuumIOLabels (cid, ByRef vacuumOn$, ByRef vacuumBreakOn$, ByRef suctionState$)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "Vacuum On = ", vacuumOn$
	Print "Vacuum Break On = ", vacuumBreakOn$
	Print "Suction State = ", suctionState$
	
	 ' Tighten
	programNo = 0
	For count = 0 To 2
		Print "Tightening count = ", count
		ret = Lib2692473628_F5 (cid, programNo)
		If ret <> 0 Then
			Exit Function
		EndIf
		Wait 1
	Next
	
	 ' Loosen
	programNo = 0
	For count = 0 To 2
		Print "Loosening count = ", count
		ret = Lib2692473628_F6 (cid, programNo)
		If ret <> 0 Then
			Exit Function
		EndIf
		Wait 1
	Next
	
	 ' Free run
	programNo = 0
	seconds = 5
	Lib2692473628_F7 (cid, programNo, seconds)
Fend
 ' Function Expr$ (flag As Boolean) As String
Function Lib2692473628_F4$ (flag As Boolean) As String
	If flag Then
		Lib2692473628_F4$ = "True"
	Else
		Lib2692473628_F4$ = "False"
	EndIf
Fend
 ' Function Tighten (cid As Integer, programNo As Integer) As Integer
Function Lib2692473628_F5 (cid As Integer, programNo As Integer) As Integer
	
	Integer ret
	Boolean isEnd, isBusy, isErr, canGet, isOK
	Integer errCode
	UShort retProgramNo
	Real numTurns, torque, timeElapsed
	UShort actualCount, samplingAngle
	
	ret = VGD_PF_StartTightening (cid, programNo)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "Start tightening"
	
	Do
		ret = VGD_PF_CheckStatus (cid, ByRef isEnd, ByRef isBusy, ByRef isErr, ByRef errCode)
		If ret <> VGD_PF_NO_ERR Then
			Exit Function
		EndIf
		Print "isEnd = ", Lib2692473628_F4$ (isEnd), " isBusy = ", Lib2692473628_F4$ (isBusy), " isErr = ", Lib2692473628_F4$ (isErr), " errCode = ", errCode
		Wait 0.1
	Loop While isBusy
	Print "End tightening"
	
	Do
		ret = VGD_PF_CanGetTighteningResult (cid, ByRef canGet)
		If ret <> VGD_PF_NO_ERR Then
			Exit Function
		EndIf
		Print "canGet = ", Lib2692473628_F4$ (canGet)
		Wait 0.01
	Loop Until canGet
	
	ret = VGD_PF_GetTighteningResult (cid, True, ByRef retProgramNo, ByRef numTurns, ByRef torque, ByRef timeElapsed, ByRef isOK, ByRef errCode)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "programNo = ", retProgramNo, "numTurns = ", numTurns, " torque = ", torque, " timeElapsed = ", timeElapsed, " isOK = ", Lib2692473628_F4$ (isOK), " errCode = ", errCode
	
	ret = VGD_PF_GetTorqueData (cid, ByRef actualCount, ByRef samplingAngle)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "actualCount = ", actualCount, " samplingAngle = ", samplingAngle
Fend
 ' Function Loosen (cid As Integer, programNo As Integer) As Integer
Function Lib2692473628_F6 (cid As Integer, programNo As Integer) As Integer
	
	Integer ret
	Boolean isEnd, isBusy, isErr
	Integer errCode
	
	ret = VGD_PF_StartLoosening (cid, programNo)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "Start loosening"
	
	Do
		ret = VGD_PF_CheckStatus (cid, ByRef isEnd, ByRef isBusy, ByRef isErr, ByRef errCode)
		If ret <> VGD_PF_NO_ERR Then
			Exit Function
		EndIf
		Print "isEnd = ", Lib2692473628_F4$ (isEnd), " isBusy = ", Lib2692473628_F4$ (isBusy), " isErr = ", Lib2692473628_F4$ (isErr), " errCode = ", errCode
		Wait 0.1
	Loop While isBusy
	Print "End loosening"
Fend
 ' Function FreeRun (cid As Integer, programNo As Integer, seconds As Integer) As Integer
Function Lib2692473628_F7 (cid As Integer, programNo As Integer, seconds As Integer) As Integer
	
	Integer ret
	Integer sec
	Boolean isEnd, isBusy, isErr
	Integer errCode
	
	ret = VGD_PF_StartFreeRun (cid, programNo)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "Start free run"
	
	For sec = 0 To seconds
		ret = VGD_PF_CheckStatus (cid, ByRef isEnd, ByRef isBusy, ByRef isErr, ByRef errCode)
		If ret <> VGD_PF_NO_ERR Then
			Exit Function
		EndIf
		Print "isEnd = ", Lib2692473628_F4$ (isEnd), " isBusy = ", Lib2692473628_F4$ (isBusy), " isErr = ", Lib2692473628_F4$ (isErr), " errCode = ", errCode
		Wait 1
	Next
	
	ret = VGD_PF_Stop (cid)
	If ret <> VGD_PF_NO_ERR Then
		Exit Function
	EndIf
	Print "End free run"
	
Fend
