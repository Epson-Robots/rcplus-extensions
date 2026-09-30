 ' ======================================================================
 '	Vanguard Systems Screwdriver Control Library
 '	Public Functions and etc.
 ' ======================================================================
#include "VGD_PF_defines.inc"
#include "VanguardModelPF.inc"
#include "Modbus.inc"
 ' ----------------------------------------------------------------------
 '	Module Variables
 ' ----------------------------------------------------------------------
 ' For mutual exclusion
NonStatic Boolean mInitialized
NonStatic Integer mLock
 ' Connection ids
NonStatic Integer mCid (Lib2692473628_G3)
 ' TCP Ports
 ' 0: Free
 ' 201Å`216: In use
NonStatic Integer mPort (Lib2692473628_G3)
 ' Log Storages
 ' 0: PC
 ' 1: USB
 ' 2: Flash
NonStatic Integer mLogStorage (Lib2692473628_G3)
 ' Log Folders  (Effective when the log storage is PC)
NonStatic String mLogFolder$ (Lib2692473628_G3)
 ' Maximum number of waveform files
NonStatic Integer mMaxNumFilesOnPass (Lib2692473628_G3)
NonStatic Integer mMaxNumFilesOnFail (Lib2692473628_G3)
 ' I/O Labels
NonStatic String mVacuumOn$ (Lib2692473628_G3)
NonStatic String mVacuumBreakOn$ (Lib2692473628_G3)
NonStatic String mSuctionState$ (Lib2692473628_G3)
 ' CT-CONTS Id
NonStatic Integer mId (Lib2692473628_G3)
 ' CT-CONTS Tool Type
NonStatic String mToolType$ (Lib2692473628_G3)
 ' Log file number
NonStatic Integer mLogFileNum (Lib2692473628_G3, 1)
 ' Tighting log synchronization
NonStatic Long mTighteningSerial (Lib2692473628_G3)
NonStatic Long mLoggedSerial (Lib2692473628_G3)
NonStatic String mLoggedStamp$ (Lib2692473628_G3)
NonStatic Boolean mLoggedIsOK (Lib2692473628_G3)
 ' Wave form file number
Global Preserve Integer Lib2692473628_G2 (Lib2692473628_G3, 2)
 ' ----------------------------------------------------------------------
 '	Funcitons
 ' ----------------------------------------------------------------------
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Bytes to string
 '
 '	Args:
 '		buf  (ByRef UByte Array): Bytes
 '		length  (Integer): Number of bytes
 '
 '	Returns:
 '		 (String): string
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function Bytes2String$ (ByRef buf () As UByte, length As Integer) As String
Function Lib2692473628_F21$ (ByRef buf () As UByte, length As Integer) As String
	
	Integer offset
	
	Lib2692473628_F21$ = ""
	For offset = 0 To length - 1
		Lib2692473628_F21$ = Lib2692473628_F21$ + Chr$ (buf (offset))
	Next
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Load definitions from the definition file.
 '
 '	Args:
 '		vgdpf_name$  (String): Definition file name  (w/o extension)
 '		cindex  (Integer): Connection index
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function LoadDefinitions (vgdpf_name$ As String, cindex As Integer) As Integer
Function Lib2692473628_F22 (vgdpf_name$ As String, cindex As Integer) As Integer
	
	OnErr GoTo ErrProc
	
	String fileName$
	Integer fileNum
	UByte buffer (200)
	Integer length, offset
	fileName$ = vgdpf_name$ + ".vgdpf"
	ChDisk FLASH
	If FileExists (fileName$) <> True Then
		GoTo ErrProc
	EndIf
	
	fileNum = FreeFile
	BOpen fileName$ As #fileNum
	
	 ' Check headers
	ReadBin #fileNum, buffer (), 16
	If  (Lib2692473628_F21$ (ByRef buffer (), 5) <> "VGDPF" Or buffer (8) <> 1 Or buffer (9) <> 0 Or buffer (10) <> 0) Then
		Lib2692473628_F8 (" [ERROR]LoadDefinitions: UNEXPECTED HEADER")
		Lib2692473628_F22 = VGD_PF_ERR_BAD_DEFINITION_FILE
		Exit Function
	EndIf
	
	 ' Get logging info
	Seek #fileNum, OFFSET_LOGGING_STORAGE_TYPE
	ReadBin #fileNum, mLogStorage (cindex)
	If mLogStorage (cindex) = 0 Then
		Seek #fileNum, OFFSET_LOGGING_FOLDER
		ReadBin #fileNum, length
		If length > UBound (buffer) Then
			Lib2692473628_F8 (" [ERROR]LoadDefinitions: UNEXPECTED PATH LENGTH")
			Lib2692473628_F22 = VGD_PF_ERR_BAD_DEFINITION_FILE
			Exit Function
		EndIf
		If length > 0 Then
			ReadBin #fileNum, buffer (), length
			mLogFolder$ (cindex) = Lib2692473628_F21$ (ByRef buffer (), length)
		EndIf
	EndIf
	Select mLogStorage (cindex)
		Case 0
			offset = OFFSET_LOGGING_MAX_NUM_FILES_PC
		Case 1
			offset = OFFSET_LOGGING_MAX_NUM_FILES_USB
		Case 2
			offset = OFFSET_LOGGING_MAX_NUM_FILES_FL
		Default
			Lib2692473628_F8 (" [ERROR]LoadDefinitions: UNEXPECTED LOG STORAGE")
			Lib2692473628_F22 = VGD_PF_ERR_BAD_DEFINITION_FILE
			Exit Function
	Send
	Seek #fileNum, offset
	ReadBin #fileNum, buffer (), 4
	mMaxNumFilesOnPass (cindex) = buffer (0) + buffer (1) * 256	 'Little Endian
	mMaxNumFilesOnFail (cindex) = buffer (2) + buffer (3) * 256	 'Little Endian
	
	 ' Get network info
	Seek #fileNum, OFFSET_NETWORK_TCPPORT
	ReadBin #fileNum, buffer (), 2
	mPort (cindex) = buffer (0) + buffer (1) * 256	 'Little Endian
	
	 ' Get I/O labels
	Seek #fileNum, OFFSET_VACUUM_IOLABEL_ON
	ReadBin #fileNum, length
	If length > 0 Then
		ReadBin #fileNum, buffer (), length
		mVacuumOn$ (cindex) = Lib2692473628_F21$ (ByRef buffer (), length)
	EndIf
	Seek #fileNum, OFFSET_VACUUM_IOLABEL_BREAK_ON
	ReadBin #fileNum, length
	If length > 0 Then
		ReadBin #fileNum, buffer (), length
		mVacuumBreakOn$ (cindex) = Lib2692473628_F21$ (ByRef buffer (), length)
	EndIf
	Seek #fileNum, OFFSET_VACUUM_IOLABEL_SUCTION
	ReadBin #fileNum, length
	If length > 0 Then
		ReadBin #fileNum, buffer (), length
		mSuctionState$ (cindex) = Lib2692473628_F21$ (ByRef buffer (), length)
	EndIf
	
	Close #fileNum
	
	Lib2692473628_F8 (" LoadDefinitions: cindex = " + Str$ (cindex))
	Lib2692473628_F22 = VGD_PF_NO_ERR
	Exit Function
ErrProc:
	Lib2692473628_F8 (" [ERROR]LoadDefinitions: FILE")
	Lib2692473628_F22 = VGD_PF_ERR_FILE
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Make log path name
 '
 '	Args:
 '		cindex  (Integer): Connection index
 '		kind  (Integer): Log kind  (0: Tightening results, 1: Error, 2: Torque wave form)
 '		wfIndex  (ByRef Integer): Variable to set wave form file selector index
 '		maxCount  (ByRef Integer): Variable to set max number of wave form files
 '		stamp$  (ByRef String): Variable to set related date and time
 '	Returns:
 '		 (String): Path name
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function LogPath$ (cindex As Integer, kind As Integer, ByRef wfIndex As Integer, ByRef maxCount As Integer, ByRef stamp$ As String) As String
Function Lib2692473628_F23$ (cindex As Integer, kind As Integer, ByRef wfIndex As Integer, ByRef maxCount As Integer, ByRef stamp$ As String) As String
	
	String fileName$, toolType$, id$, category$, wfNum$
	id$ = Right$ ("00" + Str$ (mId (cindex)), 3)
	toolType$ = mToolType$ (cindex)
	
	Select kind
		Case 0
			fileName$ = "VGD_PF_" + toolType$ + "_" + id$ + "_R.csv"
		Case 1
			fileName$ = "VGD_PF_" + toolType$ + "_" + id$ + "_E.csv"
		Case 2
			If mTighteningSerial (cindex) = mLoggedSerial (cindex) Then
				If mLoggedIsOK (cindex) Then
					category$ = "_OK_"
					wfIndex = 0
					maxCount = mMaxNumFilesOnPass (cindex)
				Else
					category$ = "_NG_"
					wfIndex = 1
					maxCount = mMaxNumFilesOnFail (cindex)
				EndIf
				stamp$ = mLoggedStamp$ (cindex)
			Else
				category$ = "_UN_"
				wfIndex = 2
				maxCount = MAX_NUM_FILES_UNKNOWN;
				stamp$ = Date$ + " " + Time$
			EndIf
			wfNum$ = Right$ ("0000" + Str$ (Lib2692473628_G2 (cindex, wfIndex)), 5)
			fileName$ = "VGD_PF_" + toolType$ + "_" + id$ + category$ + wfNum$ + ".tw"
		Default
			fileName$ = ""
	Send
	
	Select mLogStorage (cindex)
		Case 0
			Lib2692473628_F23$ = mLogFolder$ (cindex)
			If Len (Lib2692473628_F23$) > 0 Then
				Lib2692473628_F23$ = Lib2692473628_F23$ + "\"
			EndIf
			Lib2692473628_F23$ = Lib2692473628_F23$ + fileName$
		Default
			Lib2692473628_F23$ = fileName$
	Send
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Open or create log file
 '
 '	Args:
 '		cindex  (Integer): Conneciton index
 '		kind  (Integer): Log kind  (0: Tightening results, 1: Error, 2: Torque wave form)
 '		header$  (String): Header line
 '
 '	Notice:
 '		If an error happens, print a message and disable logging.
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function OpenLogFile (cindex As Integer, kind As Integer, header$ As String)
Function Lib2692473628_F24 (cindex As Integer, kind As Integer, header$ As String)
	OnErr GoTo ErrProc
	
	String path$
	Boolean isNew
	Integer fileNum
	Integer wfIndex, maxCount
	String stamp$
	
	path$ = Lib2692473628_F23$ (cindex, kind, ByRef wfIndex, ByRef maxCount, ByRef stamp$)
	Select mLogStorage (cindex)
		Case 0
			ChDisk PC
		Case 1
			ChDisk USB
		Case 2
			ChDisk FLASH
	Send
	isNew = Not FileExists (path$)
	fileNum = FreeFile
	AOpen path$ As #fileNum
	If isNew Then
		Print #fileNum, header$
	EndIf
	mLogFileNum (cindex, kind) = fileNum
	Exit Function
ErrProc:
	Print "[VGD_PF] Logging is disabled due to error. Path: ", Path$
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Log a communication error.
 '
 '	Args:
 '		cindex  (Integer): Connection index
 '		operation  (Integer): Operation on error
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function LogError (cindex As Integer, operation As Integer)
Function Lib2692473628_F25 (cindex As Integer, operation As Integer)
	
	Integer fileNum
	
	fileNum = mLogFileNum (cindex, 1)
	If fileNum <> 0 Then
		Print #fileNum, Date$ + " " + Time$, ",", operation
		Flush #fileNum
	EndIf
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Connect to CT-CONTS.
 '
 '	Args:
 '		vgdpf_name$  (String): Definition file name  (w/o extension)
 '   	cid  (ByRef Integer): Variable to set connection id
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_Connect (vgdpf_name$ As String, ByRef cid As Integer) As Integer
	_SetFunctionInfo On, 1
	Integer cindex
	Integer ret
	UShort value
	Double waitTime
	
	Randomize
	Do While Not mInitialized
		 ' System is not yet initialized
		waitTime = Rnd (1.0)
		Wait waitTime
		If Not mInitialized Then
			mLock = SyncLockReserve
			mInitialized = True
		EndIf
	Loop
	SyncLock mLock
	For cindex = 0 To MAX_NUM_CONNECTIONS
		If cindex < MAX_NUM_CONNECTIONS And mCid (cindex) = 0 Then
			mCid (cindex) = 1 + Int (Rnd (10000))
			Exit For
		EndIf
	Next
	SyncUnlock mLock
	If cindex >= MAX_NUM_CONNECTIONS Then
		ret = VGD_PF_ERR_NO_MORE_CONNECTION
		GoTo ErrProc
	EndIf
	cid = mCid (cindex)
	
	ret = Lib2692473628_F22 (vgdpf_name$, cindex)
	If ret <> SUCCESS Then
		GoTo ErrProc
	EndIf
	
	If Lib2692473628_G1 Then
		Print " ----------"
		Print " cid = ", cid
		Print " cindex = ", cindex
		Print " TCP Port = ", mPort (cindex)
		Print " Log Storage = ", mLogStorage (cindex)
		Print " Log Folder = ", mLogFolder$ (cindex)
		Print " Max Num Files On Pass = ", mMaxNumFilesOnPass (cindex)
		Print " Max Num Files On Fail = ", mMaxNumFilesOnFail (cindex)
		Print " Vacuum On = ", mVacuumOn$ (cindex)
		Print " Vacuum Break On = ", mVacuumBreakOn$ (cindex)
		Print " Suction State = ", mSuctionState$ (cindex)
		Print " ----------"
	EndIf
	ret = Lib2692473628_F10 (mPort (cindex))
	If ret <> SUCCESS Then
		ret = VGD_PF_ERR_NETWORK
		GoTo ErrProc
	EndIf
	
	ret = Lib2692473628_F20 (mPort (cindex), REG_ENABLE_REMOTE, VAL_DIO_DISABLE)
	If ret <> SUCCESS Then
		ret = VGD_PF_ERR_NETWORK
		GoTo ErrProc
	EndIf
	
	ret = Lib2692473628_F15 (ByRef value, mPort (cindex), REG_ID)
	If ret <> SUCCESS Then
		ret = VGD_PF_ERR_NETWORK
		GoTo ErrProc
	EndIf
	mId (cindex) = value
	
	ret = Lib2692473628_F15 (ByRef value, mPort (cindex), REG_TOOLTYPE);
	If ret <> SUCCESS Then
		ret = VGD_PF_ERR_NETWORK
		GoTo ErrProc
	EndIf
	Select value
		Case 0
			mToolType$ (cindex) = "S"
		Case 1
			mToolType$ (cindex) = "L"
		Case 2
			mToolType$ (cindex) = "XL"
		Case 4
			mToolType$ (cindex) = "GM"
		Case 5
			mToolType$ (cindex) = "GX"
		Default
			ret = VGD_PF_ERR_UNKNOWN_TOOL_TYPE
			GoTo ErrProc
	Send
	
	If Lib2692473628_G1 Then
		Print " ----------"
		Print " Id = ", mId (cindex)
		Print " ToolType = ", mToolType$ (cindex)
		Print " ----------"
	EndIf
	
	Lib2692473628_F24 (cindex, 0, "DateTime,ProgramNo,NumTurns,Torque,ElapsedTime,Result,ErrorDetail,WaveIndex")
	Lib2692473628_F24 (cindex, 1, "DateTime,ErrorOperation")
	
	Lib2692473628_F8 (" VGD_PF_Connect: cid = " + Str$ (cid))
	VGD_PF_Connect = VGD_PF_NO_ERR
	Exit Function
ErrProc:
	
	Lib2692473628_F8 (" [ERROR]VGD_PF_Connect: ret = " + Str$ (ret) + " cid = " + Str$ (cid))
	SyncLock mLock
	mCid (cindex) = 0
	SyncUnlock mLock
	VGD_PF_Connect = ret
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Check connection id
 '
 '	Args:
 '		cid  (Integer): connection id
 '		cindex  (ByRef Integer): Variable to set connection index
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function CheckConnectionId (cid As Integer, ByRef cindex As Integer) As Integer
Function Lib2692473628_F26 (cid As Integer, ByRef cindex As Integer) As Integer
	For cindex = 0 To MAX_NUM_CONNECTIONS - 1
		If mCid (cindex) = cid Then
			Lib2692473628_F26 = VGD_PF_NO_ERR
			Exit Function
		EndIf
	Next
	
	Lib2692473628_F26 = VGD_PF_ERR_BAD_CONNECTION_ID
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Get I/O Labels for vacuum control
 '
 '	Args:
 '		cid  (Integer): Connection id
 '		vacuumOn$  (ByRef String): Variable to set vacuum on label
 '		vacuumBreakOn$  (ByRef String): Variable to set vacuum break on label
 '		suctionState$  (ByRef String): Variable to set suction state label
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_GetVacuumIOLabels (cid As Integer, ByRef vacuumOn$ As String, ByRef vacuumBreakOn$ As String, ByRef suctionState$ As String) As Integer
	_SetFunctionInfo On, 1
	
	Integer cindex
	
	VGD_PF_GetVacuumIOLabels = Lib2692473628_F26 (cid, ByRef cindex)
	If VGD_PF_GetVacuumIOLabels = VGD_PF_NO_ERR Then
		vacuumOn$ = mVacuumOn$ (cindex)
		vacuumBreakOn$ = mVacuumBreakOn$ (cindex)
		suctionState$ = mSuctionState$ (cindex)
	EndIf
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Disconnect from CT-CONTS.
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '
 '	Returns:
 '   	 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_Disconnect (cid As Integer) As Integer
	_SetFunctionInfo On, 1
	Integer cindex
	Integer ret
	Integer fileNum
	
	ret = Lib2692473628_F26 (cid, ByRef cindex)
	If ret <> VGD_PF_NO_ERR Then
		VGD_PF_Disconnect = ret
		Exit Function
	EndIf
	
	ret = Lib2692473628_F20 (mPort (cindex), REG_ENABLE_REMOTE, VAL_DIO_DISABLE)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, VGD_PF_OP_CONNECT)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	ret = Lib2692473628_F11 (mPort (cindex))
	If ret <> SUCCESS Then
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	ret = VGD_PF_NO_ERR
EndProc:
	OnErr GoTo End
	fileNum = mLogFileNum (cindex, 0)
	If fileNum <> 0 Then
		Close #fileNum
	EndIf
	fileNum = mLogFileNum (cindex, 1)
	If fileNum <> 0 Then
		Close #fileNum
	EndIf
End:
	Lib2692473628_F8 (" VGD_PF_Disconnect: ret = " + Str$ (ret) + " cid = " + Str$ (cid))
	SyncLock mLock
	mPort (cindex) = 0
	SyncUnlock mLock
	mLogFileNum (cindex, 0) = 0
	mLogFileNum (cindex, 1) = 0
	VGD_PF_Disconnect = ret
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Get DI register value, then store program number and clear operation bits
 '	for following proccess.
 '
 '	Args:
 '   	cindex  (Integer): Connection index
 '   	programNo  (UShort): Program number
 '  		regValue  (ByRef UShort): Variable to set register value
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function MakeDIValue (cindex As Integer, programNo As UShort, ByRef regValue As UShort) As Integer
Function Lib2692473628_F27 (cindex As Integer, programNo As UShort, ByRef regValue As UShort) As Integer
	Integer ret
	
	ret = Lib2692473628_F16 (ByRef regValue, mPort (cindex), REG_DI)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, VGD_PF_OP_UNKNOWN)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	regValue =  (regValue And VAL_DI_PROGRAM_MASK) Or LShift (programNo, VAL_DI_PROGRAM_SHIFT)
	
	ret = VGD_PF_NO_ERR
EndProc:
	
	Lib2692473628_F27 = ret
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Start operation.  (Not wait end of the operation)
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '  		programNo  (UShort): Program number
 '   	operation  (UShort): Operation  (DI bit)
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function StartOperation (cid As Integer, programNo As Integer, operation As UShort) As Integer
Function Lib2692473628_F28 (cid As Integer, programNo As Integer, operation As UShort) As Integer
	Integer cindex
	Integer ret
	UShort regValue
	Integer errOpr
	
	ret = Lib2692473628_F26 (cid, ByRef cindex)
	If ret <> VGD_PF_NO_ERR Then
		GoTo EndProc
	EndIf
	
	Select operation
		Case VAL_DI_FSN
			mTighteningSerial (cindex) = mTighteningSerial (cindex) + 1
			errOpr = VGD_PF_OP_TIGHTEN
		Case VAL_DI_LSN
			errOpr = VGD_PF_OP_LOOSEN
		Case VAL_DI_RTN
			errOpr = VGD_PF_OP_FREERUN
		Default
			ret = VGD_PF_ERR_BAD_OPERATION
			GoTo EndProc
	Send
	
	 ' Reset error
	ret = Lib2692473628_F29 (cid, errOpr)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, errOpr)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	ret = Lib2692473628_F27 (cindex, programNo, ByRef regValue)
	If ret <> SUCCESS Then
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	regValue = regValue Or operation
	
	If operation = VAL_DI_FSN Then
		ret = Lib2692473628_F20 (mPort (cindex), REG_REQUEST_EXECUTE, VAL_CLEAR_CAN_GET_TIGHTEN_DATA)
		If ret <> SUCCESS Then
			Lib2692473628_F25 (cindex, errOpr)
			ret = VGD_PF_ERR_NETWORK
			GoTo EndProc
		EndIf
	EndIf
	ret = Lib2692473628_F20 (mPort (cindex), REG_ENABLE_REMOTE, VAL_ENABLE_DI)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cid, errOpr)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	ret = Lib2692473628_F20 (mPort (cindex), REG_DI, regValue)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, errOpr)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	ret = VGD_PF_NO_ERR
EndProc:
	Lib2692473628_F8 (" StartOperation: ret = " + Str$ (ret) + " cid = " + Str$ (cid) + " programNo = " + Str$ (programNo) + " operation = " + Hex$ (operation))
	Lib2692473628_F28 = ret
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Start tightening.  (Not wait end of the operation)
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '   	programNo  (UShort): Program number
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_StartTightening (cid As Integer, programNo As UShort) As Integer
	_SetFunctionInfo On, 1
	VGD_PF_StartTightening = Lib2692473628_F28 (cid, programNo, VAL_DI_FSN)
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Start loosening.  (Not wait end of the operation)
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '   	programNo  (UShort): Program number
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_StartLoosening (cid As Integer, programNo As UShort) As Integer
	_SetFunctionInfo On, 1
	VGD_PF_StartLoosening = Lib2692473628_F28 (cid, programNo, VAL_DI_LSN)
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Start free run.  (Not wait end of the operation)
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '   	programNo  (UShort): Program number
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_StartFreeRun (cid As Integer, programNo As UShort) As Integer
	_SetFunctionInfo On, 1
	VGD_PF_StartFreeRun = Lib2692473628_F28 (cid, programNo, VAL_DI_RTN)
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Stop the running operation.
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '		errOpr  (Integer): Operation for error logging
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function StopOperation (cid As Integer, errOpr As Integer) As Integer
Function Lib2692473628_F29 (cid As Integer, errOpr As Integer) As Integer
	Integer cindex
	Integer ret
	UShort regValue
	
	ret = Lib2692473628_F26 (cid, ByRef cindex)
	If ret <> VGD_PF_NO_ERR Then
		GoTo EndProc
	EndIf
	
	ret = Lib2692473628_F16 (ByRef regValue, mPort (cindex), REG_DI)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, errOpr)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	regValue = regValue And  (Not  (VAL_DI_FSN Or VAL_DI_LSN Or VAL_DI_RTN))
	
	ret = Lib2692473628_F20 (mPort (cindex), REG_ENABLE_REMOTE, VAL_ENABLE_DI)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, errOpr)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	ret = Lib2692473628_F20 (mPort (cindex), REG_DI, regValue)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, errOpr)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	ret = VGD_PF_NO_ERR
EndProc:
	Lib2692473628_F8 (" VGD_PF_Stop: ret = " + Str$ (ret) + " cid = " + Str$ (cid))
	Lib2692473628_F29 = ret
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Stop the running operation.
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_Stop (cid As Integer) As Integer
	_SetFunctionInfo On, 1
	
	VGD_PF_Stop = Lib2692473628_F29 (cid, VGD_PF_OP_STOP)
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Retrieve the operation status.
 '
 '	Args:
 '   	cid  (Integer): Conection id
 '   	isEnd  (ByRef Boolean): Variable to set END flag
 '   	isBusy  (ByRef Boolean): Variable to set BUSY flag
 '   	isErr  (ByRef Boolean): Variable to set ERR flag
 '   	errCode  (Integer): Variable to set tightening error detail, effective only when isErr is True
 '
 '	Returns:
 '		 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_CheckStatus (cid As Integer, ByRef isEnd As Boolean, ByRef isBusy As Boolean, ByRef isErr As Boolean, ByRef errCode As Integer) As Integer
	_SetFunctionInfo On, 1
	Integer cindex
	Integer ret
	UShort regValue
	
	ret = Lib2692473628_F26 (cid, ByRef cindex)
	If ret <> VGD_PF_NO_ERR Then
		GoTo EndProc
	EndIf
	
	ret = Lib2692473628_F16 (ByRef regValue, mPort (cindex), REG_DO)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, VGD_PF_OP_CHECK_STATUS)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	If regValue And VAL_DO_END Then
		isEnd = True
	Else
		IsEnd = False
	EndIf
	If regValue And VAL_DO_BUSY Then
		isBusy = True
	Else
		isBusy = False
	EndIf
	If regValue And VAL_DO_ERR Then
		isErr = True
		errCode = RShift (regValue And VAL_DO_ERRCODE_MASK, VAL_DO_ERRCODE_SHIFT)
	Else
		isErr = False
	EndIf
	
	ret = VGD_PF_NO_ERR
EndProc:
	Lib2692473628_F8 (" VGD_PF_CheckStatus: ret = " + Str$ (ret) + " cid = " + Str$ (cid))
	VGD_PF_CheckStatus = ret
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Check if the tightening result is ready to get.
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '   	canGet  (ByRef Boolean): Variable to set ready or net
 '
 '	Returns:
 '   	 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_CanGetTighteningResult (cid As Integer, ByRef canGet As Boolean) As Integer
	_SetFunctionInfo On, 1
	Integer cindex
	Integer ret
	UShort regValue
	
	ret = Lib2692473628_F26 (cid, ByRef cindex)
	If ret <> VGD_PF_NO_ERR Then
		GoTo EndProc
	EndIf
	
	ret = Lib2692473628_F15 (ByRef regValue, mPort (cindex), REG_CAN_GET_TIGHTENING_RESULT)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, VGD_PF_OP_GET_TIGHTENING_RESULT)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	If regValue = 0 Then
		canGet = False
	Else
		canGet = True
	EndIf
	
	ret = VGD_PF_NO_ERR
EndProc:
	Lib2692473628_F8 (" VGD_PF_CanGetTighteningResult: ret = " + Str$ (ret) + " cid = " + Str$ (cid))
	VGD_PF_CanGetTighteningResult = ret
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Log tightening result.
 '
 '	Args:
 '		cindex  (Integer): Communication index
 '		programNo  (UShort): Program number
 '		numTurns  (Real): Number of turns
 '		torque  (Real): Torque
 '		timeElapsed  (Real): Elapsed time
 '		isOK  (Boolean): OK  (true) or NG  (false)
 '		errCode  (Integer): Error detail
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function LogTighteningResult (cindex As Integer, programNo As UShort, numTurns As Real, torque As Real, timeElapsed As Real, isOK As Boolean, errCode As Integer)
Function Lib2692473628_F30 (cindex As Integer, programNo As UShort, numTurns As Real, torque As Real, timeElapsed As Real, isOK As Boolean, errCode As Integer)
	
	Integer fileNum
	String status$, errDetail$
	Integer wfIndex, maxCount
	
	mLoggedSerial (cindex) = mTighteningSerial (cindex)
	mLoggedStamp$ (cindex) = Date$ + " " + Time$
	mLoggedIsOK (cindex) = isOK
	fileNum = mLogFileNum (cindex, 0)
	If fileNum <> 0 Then
		If isOK Then
			status$ = "OK"
			wfIndex = 0
			maxCount = mMaxNumFilesOnPass (cindex)
			errDetail$ = ""
		Else
			status$ = "NG"
			wfIndex = 1
			maxCount = mMaxNumFilesOnFail (cindex)
			errDetail$ = Str$ (errCode)
		EndIf
		Print #fileNum, mLoggedStamp$ (cindex), ",", Str$ (programNo), ",", Str$ (numTurns), ",", Str$ (torque), ",", Str$ (timeElapsed), ",", status$, ",", errDetail$, ",", Str$ (Lib2692473628_G2 (cindex, wfIndex))
	EndIf
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Retrieve the tightening result and append them to the log.
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '       addLog  (Boolean): Logging flag, add log entry to the log file if it 's true
 '		programNo  (ByRef UShort): Program number
 '   	numTurns  (ByRef Real): Variable to set number of turns
 '   	torque  (ByRef Real): Variable to set torque  (Always mNÅEm)
 '   	timeElapsed  (ByRef Real): Variable to set elapsed time
 '   	isOK  (ByRef Boolean): Variable to set OK  (true) or NG  (false)
 '   	errCode  (ByRef Integer): Variable to set tightening error detail, effective only when isOK is False
 '
 '	Returns:
 '   	 (Integer): Error code
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_GetTighteningResult (cid As Integer, addLog As Boolean, ByRef programNo As UShort, ByRef numTurns As Real, ByRef torque As Real, ByRef timeElapsed As Real, ByRef isOK As Boolean, ByRef errCode As Integer) As Integer
	_SetFunctionInfo On, 1
	Integer cindex
	Integer ret
	UShort regValues (6)
	Real factor
	
	ret = Lib2692473628_F26 (cid, ByRef cindex)
	If ret <> VGD_PF_NO_ERR Then
		GoTo EndProc
	EndIf
	
	ret = Lib2692473628_F18 (ByRef regValues (), UBound (regValues), mPort (cindex), REG_GET_TIGHTENING_RESULT)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, VGD_PF_OP_GET_TIGHTENING_RESULT)
		ret = VGD_PF_ERR_NETWORK
		GoTo EndProc
	EndIf
	
	programNo = regValues (0)
	factor = 10
	numTurns = regValues (1) / factor
	torque = regValues (2) / factor
	factor = 1000
	timeElapsed = regValues (3) / factor
	If regValues (4) = 0 Then
		isOK = True
	Else
		isOK = False
	EndIf
	errCode = regValues (5)
	If addLog Then
		Lib2692473628_F30 (cindex, programNo, numTurns, torque, timeElapsed, isOK, errCode)
	EndIf
EndProc:
	
	Lib2692473628_F8 (" VGD_PF_GetTighteningResult: ret = " + Str$ (ret) + " cid = " + Str$ (cid))
	VGD_PF_GetTighteningResult = ret
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	String to bytes
 '
 '	Args:
 '		string$  (String): Source string
 '		buf  (ByRef UByte array): Destination buffer
 '		bufLen  (Integer): Buffer size
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 ' Function String2Bytes (string$ As String, ByRef buf () As UByte, bufLen As Integer)
Function Lib2692473628_F31 (string$ As String, ByRef buf () As UByte, bufLen As Integer)
	
	Integer offset
	String part$
	
	For offset = 0 To Len (string$) - 1
		If offset >= bufLen Then
			Exit For
		EndIf
		part$ = Mid$ (string$, 1 + offset, 1)
		buf (offset) = Asc (part$)
	Next
	
Fend
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
 '	Retrieve tighting torque wave form data and save them into a file.
 '
 '	Args:
 '   	cid  (Integer): Connection id
 '		actualCount  (ByRef UShort): Variable to set number of wave points
 '       sampleingAngle  (ByRef UShort): Variable to set sampling angle  (degree)
 '
 '  ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' ' '
Function VGD_PF_GetTorqueData (cid As Integer, ByRef actualCount As UShort, ByRef samplingAngle As UShort) As Integer
	_SetFunctionInfo On, 1
	
	OnErr GoTo ErrProc
	Integer cindex
	Integer ret
	String path$
	Integer fileNum
	UShort regValue
	UShort values (125)
	UShort count, offset, index
	UByte header (16)
	Integer wfIndex, maxCount
	String stamp$
	
	ret = Lib2692473628_F26 (cid, ByRef cindex)
	If ret <> VGD_PF_NO_ERR Then
		Lib2692473628_F8 (" [ERROR]VGD_PF_GetTorqueData: ret = " + Str$ (ret) + " cid = " + Str$ (cid))
		VGD_PF_GetTorqueData = ret
		Exit Function
	EndIf
	
	path$ = Lib2692473628_F23$ (cindex, 2, ByRef wfIndex, ByRef maxCount, ByRef stamp$)
	If maxCount <= 0 Then
		ret = VGD_PF_ERR_MAX_IS_ZERO
		Lib2692473628_F8 (" [ERROR]VGD_PF_GetTorqueData: ret = " + Str$ (ret) + " cid = " + Str$ (cid))
		VGD_PF_GetTorqueData = ret
		Exit Function
	EndIf
	Lib2692473628_G2 (cindex, wfIndex) = Lib2692473628_G2 (cindex, wfIndex) + 1
	If Lib2692473628_G2 (cindex, wfIndex) >= maxCount Then
		Lib2692473628_G2 (cindex, wfIndex) = 0
	EndIf
	ret = VGD_PF_ERR_FILE
	fileNum = FreeFile
	BOpen path$ As #fileNum
	header (0) = Asc ("P")
	header (1) = Asc ("F")
	header (2) = Asc ("T")
	header (3) = Asc ("W")
	header (4) = 1
	header (5) = 0
	header (6) = 0
	header (7) = 0
	ret = Lib2692473628_F15 (ByRef regValue, mPort (cindex), REG_NUM_TORQUES)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, VGD_PF_OP_GET_TORQUE_DATA)
		ret = VGD_PF_ERR_NETWORK
		GoTo ErrProc
	EndIf
	
	actualCount = regValue
	header (8) = actualCount And &HFF;	 ' Litten endian
	header (9) = actualCount / &H100;
	ret = Lib2692473628_F16 (ByRef regValue, mPort (cindex), REG_SAMPLING_ANGLE)
	If ret <> SUCCESS Then
		Lib2692473628_F25 (cindex, VGD_PF_OP_GET_TORQUE_DATA)
		ret = VGD_PF_ERR_NETWORK
		GoTo ErrProc
	EndIf
	samplingAngle = regValue
	header (10) = samplingAngle And &HFF	 ' Litten endian
	header (11) = samplingAngle / &H100
	ret = VGD_PF_ERR_FILE
	WriteBin #fileNum, header (), 16
	
	Lib2692473628_F31 (FmtStr$ (stamp$, "yyyymmddhhnnss"), ByRef header (), 16)
	WriteBin #fileNum, header (), 16
	
	For offset = 0 To actualCount - 1 Step 125
		If actualCount - offset > 125 Then
			count = 125
		Else
			count = actualCount - offset
		EndIf
		If count = 0 Then
			Exit For
		EndIf
		ret = Lib2692473628_F18 (ByRef values (), count, mPort (cindex), REG_TORQUES + offset)
		If ret <> SUCCESS Then
			Lib2692473628_F25 (cindex, VGD_PF_OP_GET_TORQUE_DATA)
			ret = VGD_PF_ERR_NETWORK
			GoTo ErrProc
		EndIf
		ret = VGD_PF_ERR_FILE
		For index = 0 To count - 1
			WriteBin #fileNum, values (index) And &HFF
			WriteBin #fileNum, values (index) / &H100
		Next
	Next
	Close #fileNum
	
	Lib2692473628_F8 (" VGD_PF_GetTorqueData: cid = " + Str$ (cid) + " actualCount = " + Str$ (actualCount) + " samplingAngle = " + Str$ (samplingAngle))
	VGD_PF_GetTorqueData = VGD_PF_NO_ERR
	Exit Function
ErrProc:
	OnErr GoTo EndProc
	
	Close #fileNum
	Del path$
	
EndProc:
	Lib2692473628_F8 (" [ERROR]VGD_PF_GetTorqueData: ret = " + Str$ (ret) + " cid = " + Str$ (cid) + " actualCount = " + Str$ (actualCount) + " samplingAngle = " + Str$ (samplingAngle))
	VGD_PF_GetTorqueData = ret
Fend
