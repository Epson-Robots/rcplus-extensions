' ======================================================================
'	Modbus TCP Definitions
' ======================================================================
' ----------------------------------------------------------------------
'	Error Codes
' ----------------------------------------------------------------------
' No error
#define SUCCESS 0
' General error
#define ERR_TCP_GENERAL &H01
' Timeout
#define ERR_TCP_TIMEOUT &H02
' Format error
#define ERR_TCP_FORMAT &H03
' Parameter out of range
#define ERR_MODBUS_PARAM_OUT_OF_RANGE &H10
' Invalid function code
#define ERR_MODBUS_INVALID_FUNCTION_CODE &H11
' Invalid address
#define ERR_MODBUS_INVALID_ADDRESS &H12
' Invalid register
#define ERR_MODBUS_INVALID_REGISTER &H13
' Unexpected response
#define ERR_MODBUS_RESPONSE &H14
