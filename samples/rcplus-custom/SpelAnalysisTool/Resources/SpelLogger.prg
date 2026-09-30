Global Boolean g_SpelLog_IsEnabled_
Global Boolean g_SpelLog_IsCollectTorque_
Global Real g_SpelLog_Interval_ 'seconds
Global String g_SpelLog_FilenameInfo_$
Global String g_SpelLog_FilenameSection_$
Global String g_SpelLog_FilenameRobot_$
Global Integer g_SpelLog_SyncNo_
Global Integer g_SpelLog_TimerNo_
Global Integer g_SpelLog_CycleNo_
Global String g_SpelLog_SectionName_$
Global Integer g_SpelLog_Running_
Global Integer g_SpelLog_MotionLogging_

' Initialize logger.
Function SpelLog_Init(isEnabled As Boolean, interval As Real, filename$ As String, isCollectTorque As Boolean)
	Integer fileNum
	
	g_SpelLog_IsEnabled_ = isEnabled
	g_SpelLog_IsCollectTorque_ = isCollectTorque
	
	If g_SpelLog_IsEnabled_ = False Then Exit Function
	
	g_SpelLog_Interval_ = interval
	g_SpelLog_FilenameInfo_$ = filename$ + "_Info.csv"
	g_SpelLog_FilenameSection_$ = filename$ + "_Section.csv"
	g_SpelLog_FilenameRobot_$ = filename$ + "_Motion.csv"
	g_SpelLog_CycleNo_ = 0
	g_SpelLog_SectionName_$ = ""
	
	SpelLogPrv_WriteInfoLog
	
	fileNum = FreeFile
	WOpen g_SpelLog_FilenameSection_$ As #fileNum
	Print #fileNum, "DateTime,ElapsedTime,CycleNo,SectionName,Flag"
	Close #fileNum
	
	g_SpelLog_SyncNo_ = 62
	g_SpelLog_TimerNo_ = 62
	TmReset (g_SpelLog_TimerNo_)
	g_SpelLog_Running_ = 1
	g_SpelLog_MotionLogging_ = 0
	Xqt SpelLogPrv_WriteMotionLogProc
	SpelLogPrv_WriteSectionLog("LogStart")
Fend

' Finalize logger.
Function SpelLog_Fin
	If g_SpelLog_IsEnabled_ = False Then Exit Function
	SpelLogPrv_WriteSectionLog("LogEnd")
	g_SpelLog_Running_ = 0
Fend

' Start cycle.
Function SpelLog_CycleStart
	If g_SpelLog_IsEnabled_ = False Then Exit Function
	g_SpelLog_CycleNo_ = g_SpelLog_CycleNo_ + 1
	SpelLogPrv_WriteSectionLog("CycleStart")
Fend

' End cycle.
Function SpelLog_CycleEnd
	If g_SpelLog_IsEnabled_ = False Then Exit Function
	SpelLogPrv_WriteSectionLog("CycleEnd")
Fend

' Start section.
Function SpelLog_SectionStart(sectionName$ As String)
	If g_SpelLog_IsEnabled_ = False Then Exit Function
	g_SpelLog_SectionName_$ = sectionName$
	SpelLogPrv_WriteSectionLog("SectionStart")
Fend

' End section.
Function SpelLog_SectionEnd(sectionName$ As String)
	If g_SpelLog_IsEnabled_ = False Then Exit Function
	SpelLogPrv_WriteSectionLog("SectionEnd")
	g_SpelLog_SectionName_$ = ""
Fend

' Start motion logging.
Function SpelLog_MotionStart
	If g_SpelLog_IsEnabled_ = False Then Exit Function
	g_SpelLog_MotionLogging_ = 1
Fend

' End motion logging.
Function SpelLog_MotionEnd
	If g_SpelLog_IsEnabled_ = False Then Exit Function
	g_SpelLog_MotionLogging_ = 0
Fend

Function SpelLogPrv_CreateCommonRecordData$ As String
	SpelLogPrv_CreateCommonRecordData$ = Date$ + " " + Time$ + "," + FmtStr$(Tmr(g_SpelLog_TimerNo_), "0.000") + "," + Str$(g_SpelLog_CycleNo_) + "," + g_SpelLog_SectionName_$ + ","
Fend

' Info log procedure.
Function SpelLogPrv_WriteInfoLog
	Integer fileNum
	Integer toolNo
	String record$
	
	toolNo = Tool
	
	fileNum = FreeFile
	WOpen g_SpelLog_FilenameInfo_$ As #fileNum
	Print #fileNum, "Name,Value"
	Print #fileNum, "DateTime", ",", Date$, " ", Time$
	Print #fileNum, "CtrlFWVer", ",", CtrlInfo(9)
	Print #fileNum, "RobotName", ",", RobotInfo$(0)
	Print #fileNum, "RobotType", ",", RobotType
	Print #fileNum, "RobotModelName", ",", RobotInfo$(1)
	Print #fileNum, "RobotPtFile", ",", RobotInfo$(2)
	Print #fileNum, "RobotSN", ",", RobotInfo$(4)
	Print #fileNum, "Tool", ",", toolNo
	If toolNo >= 1 Then
		Print #fileNum, "TLX", ",", FmtStr$(CX(TLSet(toolNo)), "0.000")
		Print #fileNum, "TLY", ",", FmtStr$(CY(TLSet(toolNo)), "0.000")
		Print #fileNum, "TLZ", ",", FmtStr$(CZ(TLSet(toolNo)), "0.000")
		Print #fileNum, "TLU", ",", FmtStr$(CU(TLSet(toolNo)), "0.000")
		Print #fileNum, "TLV", ",", FmtStr$(CV(TLSet(toolNo)), "0.000")
		Print #fileNum, "TLW", ",", FmtStr$(CW(TLSet(toolNo)), "0.000")
	Else
		Print #fileNum, "TLX", ",", FmtStr$(0, "0.000")
		Print #fileNum, "TLY", ",", FmtStr$(0, "0.000")
		Print #fileNum, "TLZ", ",", FmtStr$(0, "0.000")
		Print #fileNum, "TLU", ",", FmtStr$(0, "0.000")
		Print #fileNum, "TLV", ",", FmtStr$(0, "0.000")
		Print #fileNum, "TLW", ",", FmtStr$(0, "0.000")
	EndIf
	Close #fileNum
Fend
	
' Section log procedure.
Function SpelLogPrv_WriteSectionLog(flag$ As String)
	Integer fileNum
	String record$
	
	SyncLock g_SpelLog_SyncNo_
	fileNum = FreeFile
	AOpen g_SpelLog_FilenameSection_$ As #fileNum
	record$ = SpelLogPrv_CreateCommonRecordData$
	record$ = record$ + flag$
	Print #fileNum, record$
	Close #fileNum
	SyncUnlock g_SpelLog_SyncNo_
Fend

' Motion log procedure.
Function SpelLogPrv_WriteMotionLogProc
	Integer fileNum
	String record$
	Integer i
	
	fileNum = FreeFile
	WOpen g_SpelLog_FilenameRobot_$ As #fileNum
	Print #fileNum, "DateTime,ElapsedTime,CycleNo,SectionName,X,Y,Z,U,V,W,J1,J2,J3,J4,J5,J6,Torque1,Torque2,Torque3,Torque4,Torque5,Torque6,TCPSpeed"
	
	Do While g_SpelLog_Running_ = 1
		
		Wait g_SpelLog_Running_ = 0 Or g_SpelLog_MotionLogging_ = 1
		
		If g_SpelLog_MotionLogging_ = 1 Then
			P900 = RealPos
			
			record$ = SpelLogPrv_CreateCommonRecordData$
			record$ = record$ + FmtStr$(CX(P900), "0.000") + ","
			record$ = record$ + FmtStr$(CY(P900), "0.000") + ","
			record$ = record$ + FmtStr$(CZ(P900), "0.000") + ","
			record$ = record$ + FmtStr$(CU(P900), "0.000") + ","
			record$ = record$ + FmtStr$(CV(P900), "0.000") + ","
			record$ = record$ + FmtStr$(CW(P900), "0.000") + ","
			
			For i = 1 To 6
				record$ = record$ + FmtStr$(Agl(i), "0.000") + ","
			Next
			
			For i = 1 To 6
				If g_SpelLog_IsCollectTorque_ Then
					record$ = record$ + FmtStr$(RealTorque(i), "0.000") + ","
				Else
					record$ = record$ + "0.000" + ","
				EndIf
			Next
			
			record$ = record$ + FmtStr$(TCPSpeed, "0.000") + ","
			
			Print #fileNum, record$
		EndIf
		
		Wait g_SpelLog_Interval_
	Loop
	
	Close #fileNum
Fend
