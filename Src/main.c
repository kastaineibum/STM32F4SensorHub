/* USER CODE BEGIN Header */

/**
  * Board Designer				3mn.net demotree.vip  
	* All code between comment pairs USER CODE BEGIN and USER CODE END 
	* are written by Board Designer.
	* These code under [3mn.net ADDITIONAL LICENSE] and [GNU AGPL] described in http://3mn.net/wp/?p=3206
	* Copyright (c) 2019 Beijing Lizishu Tech Co.,Ltd.
  * All rights reserved.
  */
	
/**
  * This notice applies to any and all portions of this file
  * that are not between comment pairs USER CODE BEGIN and
  * USER CODE END. Other portions of this file, whether 
  * inserted by the user or by software development tools
  * are owned by their respective copyright owners.
  *
  * Copyright (c) 2018 STMicroelectronics International N.V. 
  * All rights reserved.
  *
  * Redistribution and use in source and binary forms, with or without 
  * modification, are permitted, provided that the following conditions are met:
  *
  * 1. Redistribution of source code must retain the above copyright notice, 
  *    this list of conditions and the following disclaimer.
  * 2. Redistributions in binary form must reproduce the above copyright notice,
  *    this list of conditions and the following disclaimer in the documentation
  *    and/or other materials provided with the distribution.
  * 3. Neither the name of STMicroelectronics nor the names of other 
  *    contributors to this software may be used to endorse or promote products 
  *    derived from this software without specific written permission.
  * 4. This software, including modifications and/or derivative works of this 
  *    software, must execute solely and exclusively on microcontroller or
  *    microprocessor devices manufactured by or for STMicroelectronics.
  * 5. Redistribution and use of this software other than as permitted under 
  *    this license is void and will automatically terminate your rights under 
  *    this license. 
  *
  * THIS SOFTWARE IS PROVIDED BY STMICROELECTRONICS AND CONTRIBUTORS "AS IS" 
  * AND ANY EXPRESS, IMPLIED OR STATUTORY WARRANTIES, INCLUDING, BUT NOT 
  * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
  * PARTICULAR PURPOSE AND NON-INFRINGEMENT OF THIRD PARTY INTELLECTUAL PROPERTY
  * RIGHTS ARE DISCLAIMED TO THE FULLEST EXTENT PERMITTED BY LAW. IN NO EVENT 
  * SHALL STMICROELECTRONICS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
  * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
  * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, 
  * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
  * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
  * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
  * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  *
  ******************************************************************************
  */
	
/**
  * Board Designer				3mn.net demotree.vip  
	* All code between comment pairs USER CODE BEGIN and USER CODE END 
	* are written by Board Designer.
	* 
	* Copyright (c) 2019 Beijing Lizishu Tech Co.,Ltd.
  * All rights reserved.
  */
	
/* USER CODE END Header */

/* Includes ------------------------------------------------------------------*/
#include "main.h"
#include "cmsis_os.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include "dwt_stm32_delay.h"
#include "ov5640.h"
#include "stdio.h"
#include "stdlib.h"
#include "string.h"
#include "llgpioiic.h"
#include "math.h"
#include "inv_mpu_dmp_motion_driver.h"
#include "inv_mpu.h"
#include "bmp280.h"
#include "dht11.h"
#include "key_value.h"
#include "modbus.h"
#include "motor.h"
#include "vl53l1_platform.h"
#include "vl53l1_api.h"

//#include "stm32f4xx_hal.h"
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */

/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
I2C_HandleTypeDef hi2c2;

TIM_HandleTypeDef htim1;
TIM_HandleTypeDef htim2;
TIM_HandleTypeDef htim3;
TIM_HandleTypeDef htim4;

UART_HandleTypeDef huart1;
UART_HandleTypeDef huart2;
UART_HandleTypeDef huart3;

osThreadId defaultTaskHandle;
/* USER CODE BEGIN PV */

//////////////////Task Handle
osThreadId UITask1Handle;
osThreadId UITask2Handle;
osThreadId UITask3Handle;
osThreadId UITask4Handle;
osThreadId UITask5Handle;
osThreadId UITask6Handle;
osThreadId UITask7Handle;
osThreadId UITask8Handle;
osThreadId UITask9Handle;
osThreadId UITask10Handle;
osThreadId UITask11Handle;
osThreadId UITask12Handle;

//////////////////Global Vars
int dbgi;  		//debug counter
HAL_StatusTypeDef hst;
int defaulttaskover=0;
int tftaskstarted=0;
int camerataskstarted=0;
int memstaskstarted=0;
int bmptaskstarted=0;
int keyvaluetaskstarted=0;
int dhttaskstarted=0;
int consoletaskstarted=0;
int flowmetertaskstarted=0;
int intelmodtaskstarted=0;
int uart2taskstarted=0;
int discovertaskstarted=0;
int vl53taskstarted=0;

uint32_t cpuid[3]; //cpu id
char cpuidstr[64]; //cpu id string
char *pcpuidstr; //cpu id string pointer
uint32_t tempi; //temp calculate number
uint32_t commkey; //key for server communication
uint32_t kvcommkey; //key saved in flash
uint32_t sendsequence; //sequence number of package sending
uint32_t commkey2; //key for check
uint32_t skipflag=0; 

uint32_t flowmeterntf=0;
uint32_t bmpdhtntf=0;
uint32_t baro=0;
uint32_t elevation=0;
uint32_t temperature=0;
uint32_t humidity=0;
uint32_t distance=0;

SemaphoreHandle_t uart2sph; //modbus send semaphore
SemaphoreHandle_t uart3sph; //modbus send semaphore
SemaphoreHandle_t modbusdatasph; //modbus buff semaphore

//////////////////Camera OV5640
uint8_t Com1SendFlag;
uint8_t ovx_mode=0;							  //bit0:0,RGB565;1,JPEG
#define jpeg_buf_size 16*1024  		//31*1024
__align(4) uint32_t jpeg_buf[jpeg_buf_size];
volatile uint32_t jpeg_data_len=0;
volatile uint8_t jpeg_data_ok=0;
const uint16_t jpeg_img_size_tbl[][2]=
{
	160,120,	//QQVGA 
	320,240,	//QVGA  
	640,480,	//VGA
	800,600,	//SVGA
	1024,768,	//XGA
	1280,800,	//WXGA 
	1440,900,	//WXGA+
	1280,1024,	//SXGA
	1600,1200,	//UXGA	
	1920,1080,	//1080P
	2048,1536,	//QXGA
	2592,1944,	//500W 
}; 
const char*EFFECTS_TBL[7]={"Normal","Cool","Warm","B&W","Yellowish ","Inverse","Greenish"}; 
const char*JPEG_SIZE_TBL[12]={"QQVGA","QVGA","VGA","SVGA","XGA","WXGA","WXGA+","SXGA","UXGA","1080P","QXGA","500W"};
uint32_t i,jpgstart,jpglen; 
uint8_t *p;
uint8_t headok=0;
uint8_t effect=0,contrast=2;
uint8_t size=1;			  //2=VGA 640*480
uint8_t msgbuf[15];		// 

//////////////////exFAT TF Card
/*
FATFS fs;                 //Work area (file system object) for logical drive
FIL fil;                  //file objects
uint32_t byteswritten;    //file write counts
uint32_t bytesread;       //file read counts
char wtext1[] = "This is STM32 working with exFat."; //file write buffer
uint8_t rtext1[100];                     //file read buffer
TCHAR filename[] = _T("STM32CubeMX.txt");
void* pt;
*/

//////////////////TDK MEMS MPU9250
#define q30  1073741824.0f
#define PRINT_ACCEL     (0x01)
#define PRINT_GYRO      (0x02)
#define PRINT_QUAT      (0x04)
#define ACCEL_ON        (0x01)
#define GYRO_ON         (0x02)
#define MOTION          (0)
#define NO_MOTION       (1)
#define DEFAULT_MPU_HZ  (100)
#define FLASH_SIZE      (512)
#define FLASH_MEM_START ((void*)0x1800)
uint8_t mpuinitresult = 0;
float q0=1.0f,q1=0.0f,q2=0.0f,q3=0.0f;
uint8_t show_tab[]={"0123456789"};
float Pitch,Roll,Yaw;
int temp;
struct rx_s 
{
    unsigned char header[3];
    unsigned char cmd;
};
struct hal_s 
{
    unsigned char sensors;
    unsigned char dmp_on;
    unsigned char wait_for_tap;
    volatile unsigned char new_gyro;
    unsigned short report;
    unsigned short dmp_features;
    unsigned char motion_int_mode;
    struct rx_s rx;
};
static struct hal_s hal = {0};
volatile unsigned char rx_new;
static signed char gyro_orientation[9] = {-1, 0, 0,
                                           0,-1, 0,
                                           0, 0, 1};                                  
enum packet_type_e
{
    PACKET_TYPE_ACCEL,
    PACKET_TYPE_GYRO,
    PACKET_TYPE_QUAT,
    PACKET_TYPE_TAP,
    PACKET_TYPE_ANDROID_ORIENT,
    PACKET_TYPE_PEDO,
    PACKET_TYPE_MISC
};
unsigned long sensor_timestamp;
short gyro[3], accel[3], sensors;
unsigned char more;
long quat[4];
#define  Pitch_error  -0.1 //1.0
#define  Roll_error   -0.9 //-2.0
#define  Yaw_error    0.0

//////////////////Barometer BMP280
float bmp280_temp;
float bmp280_press;
float height;
uint16_t pbmp,tbmp,hbmp;
uint32_t rawpressure;
uint32_t rawtemperature;

//////////////////Rangesensor VL53L1X	  
VL53L1_Dev_t dev53;
//VL53L1_DevData_t dt53;
VL53L1_RangingMeasurementData_t mdg;
int refreshvl53l1xflag;

//////////////////Flowmeter (square wave to TIM1-4)
int32_t flowread1; //read current flowmeter count directly from TIM1
int32_t flowreadlast1; //last count read directly from TIM1
uint32_t flowbase1; //multiple of 65536
uint32_t flowcount1; //flowmeter count
uint32_t markrestart1; //restart when flowcount nearly overflow
uint32_t tolerance1; //tolerance set by server
uint32_t thistimestartflowcount1; //flow count value when this time valve open start
uint32_t thistimecount1; //this time flow count
uint32_t valvecontrol1; //1=open 0=close

int32_t flowread2; //read current flowmeter count directly from TIM2
int32_t flowreadlast2; //last count read directly from TIM2
uint32_t flowbase2; //multiple of 65536
uint32_t flowcount2; //flowmeter count
uint32_t markrestart2; //restart when flowcount nearly overflow
uint32_t tolerance2; //tolerance set by server
uint32_t thistimestartflowcount2; //flow count value when this time valve open start
uint32_t thistimecount2; //this time flow count
uint32_t valvecontrol2; //1=open 0=close

int32_t flowread3; //read current flowmeter count directly from TIM3
int32_t flowreadlast3; //last count read directly from TIM3
uint32_t flowbase3; //multiple of 65536
uint32_t flowcount3; //flowmeter count
uint32_t markrestart3; //restart when flowcount nearly overflow
uint32_t tolerance3; //tolerance set by server
uint32_t thistimestartflowcount3; //flow count value when this time valve open start
uint32_t thistimecount3; //this time flow count
uint32_t valvecontrol3; //1=open 0=close

int32_t flowread4; //read current flowmeter count directly from TIM4
int32_t flowreadlast4; //last count read directly from TIM4
uint32_t flowbase4; //multiple of 65536
uint32_t flowcount4; //flowmeter count
uint32_t markrestart4; //restart when flowcount nearly overflow
uint32_t tolerance4; //tolerance set by server
uint32_t thistimestartflowcount4; //flow count value when this time valve open start
uint32_t thistimecount4; //this time flow count
uint32_t valvecontrol4; //1=open 0=close

//////////////////Console UART1
uint8_t tempdata[128]; //receive data area for uart1
uint8_t *ptempdata; //data pointer for uart1
char tempstr[1024]; //display string area for uart1
char *ptempstr; //string pointer for uart1
uint32_t displayflag; //string should be display in future for uart1
uint8_t *ptemp; //temp pointer for uart1
char *ptsub; //split string pointer for uart1
char argstack[4][64]; //command arg stack for uart1

uint8_t *pts; //temp pointer for command parse

//////////////////NUC USB2.0 UART3
uint8_t mbdata[256]; //modbus receive data area for uart3
uint8_t *pmbdata; //modbus data pointer for uart3
uint32_t mbtick; //modbus recv data bytes counter for uart3
uint32_t uart3errtick; //uart3 receiver error tick
uint32_t mblaststamp; //modbus stamp for RTU ADU recognizing
uint8_t mdataf[256]; //data processing buffer for modbus
uint8_t *ppmbdata; //modbus data pointer for uart3
uint32_t mblastdealstamp; //data deal stamp
uint32_t mbaddr; //self modbus address
uint32_t tmbaddr; //modbus address buffer

//////////////////485 UART2
uint8_t modbusdata[256]; //modbus receive data area for uart2
uint8_t *pmodbusdata; //modbus data pointer for uart2
uint32_t modbustick; //modbus recv data bytes counter for uart2
uint32_t uart2errtick; //uart2 receiver error tick
uint32_t modbuslaststamp; //modbus stamp for RTU ADU recognizing
uint8_t pmdata[256]; //data processing buffer for modbus
uint8_t *ppmdata; //modbus data pointer for uart2
uint32_t lastdealstamp; //data deal stamp
uint32_t modbusaddr; //self modbus address
uint32_t tempaddr; //modbus address buffer
float otherboarddata[16][255]; //other boards sensor data receive from uart2 modbus. [][254]is modbus address
uint8_t readingflag; //modbus device reading state 1 reading 0 read over
uint8_t discoverflag; //other board discover task flag

///////////////////Motor
uint16_t motorpin[16][8];


/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);
static void MX_GPIO_Init(void);
static void MX_I2C2_Init(void);
static void MX_TIM1_Init(void);
static void MX_TIM2_Init(void);
static void MX_TIM3_Init(void);
static void MX_TIM4_Init(void);
static void MX_USART1_UART_Init(void);
static void MX_USART2_UART_Init(void);
static void MX_USART3_UART_Init(void);
void StartDefaultTask(void const * argument);

/* USER CODE BEGIN PFP */
static  unsigned short inv_row_2_scale(const signed char *row);
static  unsigned short inv_orientation_matrix_to_scalar(const signed char *mtx);
static void run_self_test(void);
static void MX_GPIO_MPU9250IIC(void);
//static void MX_GPIO_OV5640SCCB(void);
static void MX_GPIO_DHT11(void);
void StartUITask1(void const * argument);
void StartUITask2(void const * argument);
void StartUITask3(void const * argument);
void StartUITask4(void const * argument);
void StartUITask5(void const * argument);
void StartUITask6(void const * argument);
void StartUITask7(void const * argument);
void StartUITask8(void const * argument);
void StartUITask9(void const * argument);
void StartUITask10(void const * argument);
void StartUITask11(void const * argument);
void StartUITask12(void const * argument);
void reboot_times_history_info( void );
//int fputc(int ch,FILE *f);
void setTolerance1(uint32_t tl);
void setTolerance2(uint32_t tl);
void setTolerance3(uint32_t tl);
void setTolerance4(uint32_t tl);
uint32_t gettargetdata(uint32_t modaddr,uint32_t sensorreg);
uint8_t gettargetiostate(uint32_t modaddr,uint16_t pinnum);
uint8_t getwritebackiostate(uint32_t modaddr,uint16_t pinnum);

void refreshvl53l1x(void);
void refreshmpu9250(void);
void refreshbmp280(void);
void refreshdht11(void);
void readothertargetdata2u2(uint32_t modaddr,uint32_t sensorreg);
void readothertargetcoil2u2(uint32_t modaddr,uint16_t pinnum);
void writeothertargetcoil2u2(uint32_t modaddr,uint16_t pinnum,uint16_t pinstate);
void sendselfdata2u2(uint32_t sensorreg);
void sendselfdata2u3(uint32_t sensorreg);
void sendselfiostate2u2(uint16_t pinnum);
void sendselfiostate2u3(uint16_t pinnum);
void sendotherdata2u3(uint32_t modaddr,uint32_t regaddr,uint32_t sensordata);
void sendotheriostate2u3(uint32_t modaddr,uint16_t pinnum,uint16_t pinstate);

/* USER CODE END PFP */

/* Private user code ---------------------------------------------------------*/
/* USER CODE BEGIN 0 */

#ifdef __GNUC__
#define PUTCHAR_PROTOTYPE int __io_putchar(int ch)
#else
#define PUTCHAR_PROTOTYPE int fputc(int ch, FILE *f)
#endif
PUTCHAR_PROTOTYPE
{
    HAL_UART_Transmit(&huart1 , (uint8_t *)&ch, 1 , 0x3ff);
    return ch;
}

/* USER CODE END 0 */

/**
  * @brief  The application entry point.
  * @retval int
  */
int main(void)
{
  /* USER CODE BEGIN 1 */
					motor_initpin(1,0,77);
					motor_initpin(1,1,78);
					motor_initpin(1,2,79);
					motor_initpin(1,3,80);
					motor_initpin(1,6,30);
					motor_initpin(1,7,31);
	
	setioout(85);
	setioout(86);
	setioout(87);
	setioout(88);
	setioout(90);
	setioout(91);
	setioout(92);
	setioout(93);
	setioout(95);
	setioout(96);
	setioout(98);
	setioout(1);
	setioout(2);
	setioout(3);
	setioout(4);
	setioout(5);
	setioout(7);
	setioout(15);
	setioout(16);
	setioout(17);
	setioout(18);
	setioout(24);
	
	setiolow(85);
	setiolow(86);
	setiolow(87);
	setiolow(88);
	setiolow(90);
	setiolow(91);
	setiolow(92);
	setiolow(93);
	setiolow(95);
	setiolow(96);
	setiolow(98);
	setiolow(1);
	setiolow(2);
	setiolow(3);
	setiolow(4);
	setiolow(5);
	setiolow(7);
	setiolow(15);
	setiolow(16);
	setiolow(17);
	setiolow(18);
	setiolow(24);
	
	ptempdata = tempdata;
	ptempstr = tempstr;
	displayflag = 0;
	
	pmodbusdata = modbusdata;
	modbustick = 0;
	uart2errtick = 0;
	modbuslaststamp = get_cyccnt();
	ppmdata = pmdata;
	lastdealstamp = modbuslaststamp;
	modbusaddr = MOD_ADDR;
	readingflag = 0;
	discoverflag = 0;
	
	pmbdata=mbdata;
	mbtick=0;
	uart3errtick = 0;
	mblaststamp = get_cyccnt();
	ppmbdata = mdataf;
	mblastdealstamp = mblaststamp;
	mbaddr = MOD_ADDR;
	
	memset(ptempdata,0,128);
	memset(ptempstr,0,1024);
	memset(pmodbusdata,0,256);
	memset(ppmdata,0,256);
	for(int m=0;m<16;m++)
	{
		for(int n=0;n<255;n++)
		{
			otherboarddata[m][n]=0;
		}
	}
	
	pcpuidstr = cpuidstr;
	
  cpuid[2] = *(__IO uint32_t*)(0x1fff7a10);//(0X1FFFF7E8);
  cpuid[1] = *(__IO uint32_t*)(0x1fff7a14);//(0X1FFFF7EC);
  cpuid[0] = *(__IO uint32_t*)(0x1fff7a18);//(0X1FFFF7F0);
	
  uint8_t temp[12];
	temp[0] = (uint8_t)(cpuid[2] & 0x000000FF);
	temp[1] = (uint8_t)((cpuid[2] & 0x0000FF00)>>8);
	temp[2] = (uint8_t)((cpuid[2] & 0x00FF0000)>>16);
	temp[3] = (uint8_t)((cpuid[2] & 0xFF000000)>>24);
	temp[4] = (uint8_t)(cpuid[1] & 0x000000FF);
	temp[5] = (uint8_t)((cpuid[1] & 0x0000FF00)>>8);
	temp[6] = (uint8_t)((cpuid[1] & 0x00FF0000)>>16);
	temp[7] = (uint8_t)((cpuid[1] & 0xFF000000)>>24);
	temp[8] = (uint8_t)(cpuid[0] & 0x000000FF);
	temp[9] = (uint8_t)((cpuid[0] & 0x0000FF00)>>8);
	temp[10] = (uint8_t)((cpuid[0] & 0x00FF0000)>>16);
	temp[11] = (uint8_t)((cpuid[0] & 0xFF000000)>>24);
	sprintf(pcpuidstr,"336D6E2E-%.2X%.2X-%.2X%.2X-%.2X%.2X-%.2X%.2X%.2X%.2X%.2X%.2X",temp[0],temp[1],temp[2],temp[3],temp[4],temp[5],temp[6],temp[7],temp[8],temp[9],temp[10],temp[11]);
	
	ptempstr = &cpuidstr[28];
	sscanf(ptempstr, "%x", &commkey);
	commkey = commkey ^ 0x336d6e2e;
	ptempstr = tempstr;
	
	uart2sph = xSemaphoreCreateMutex();
	uart3sph = xSemaphoreCreateMutex();
	modbusdatasph = xSemaphoreCreateMutex();
	
	refreshvl53l1xflag = 0;
	
  /* USER CODE END 1 */
  

  /* MCU Configuration--------------------------------------------------------*/

  /* Reset of all peripherals, Initializes the Flash interface and the Systick. */
  HAL_Init();

  /* USER CODE BEGIN Init */

  /* USER CODE END Init */

  /* Configure the system clock */
  SystemClock_Config();

  /* USER CODE BEGIN SysInit */

  /* USER CODE END SysInit */

  /* Initialize all configured peripherals */
  MX_GPIO_Init();
  MX_I2C2_Init();
  MX_TIM1_Init();
  MX_TIM2_Init();
  MX_TIM3_Init();
  MX_TIM4_Init();
  MX_USART1_UART_Init();
  MX_USART2_UART_Init();
  MX_USART3_UART_Init();
  /* USER CODE BEGIN 2 */

  if(DWT_Delay_Init())
	{
		Error_Handler(); /* Call Error Handler */
	}
	
	//rem
	// stm32f4xx_hal_uart.c line 1033 1082 passed
	// transmit lock bug fix
	////////////////////////////////////
		
	//Test IO
	//HAL_GPIO_WritePin(GPIOC, GPIO_PIN_5,GPIO_PIN_SET);//CRL1
	
	//HAL_UART_Transmit(&huart3, ptempdata, 1, 0x3ff);

	
  /* USER CODE END 2 */

  /* USER CODE BEGIN RTOS_MUTEX */
  /* add mutexes, ... */
  /* USER CODE END RTOS_MUTEX */

  /* USER CODE BEGIN RTOS_SEMAPHORES */
  /* add semaphores, ... */
  /* USER CODE END RTOS_SEMAPHORES */

  /* USER CODE BEGIN RTOS_TIMERS */
  /* start timers, add new ones, ... */
  /* USER CODE END RTOS_TIMERS */

  /* USER CODE BEGIN RTOS_QUEUES */
  /* add queues, ... */
  /* USER CODE END RTOS_QUEUES */

  /* Create the thread(s) */
  /* definition and creation of defaultTask */
  osThreadDef(defaultTask, StartDefaultTask, osPriorityNormal, 0, 128);
  defaultTaskHandle = osThreadCreate(osThread(defaultTask), NULL);

  /* USER CODE BEGIN RTOS_THREADS */
  /* add threads, ... */
	
	//defaultTask																									
	// mount tf card and blink led
  osThreadDef(UITask1, StartUITask1, osPriorityNormal, 1, 128); // ov5640 camera get jpeg
  UITask1Handle = osThreadCreate(osThread(UITask1), NULL);
  osThreadDef(UITask2, StartUITask2, osPriorityNormal, 1, 128); // mpu9250 mems dmp read
  UITask2Handle = osThreadCreate(osThread(UITask2), NULL);
  osThreadDef(UITask3, StartUITask3, osPriorityHigh, 1, 256);	// tf card file read write
  UITask3Handle = osThreadCreate(osThread(UITask3), NULL);
	osThreadDef(UITask4, StartUITask4, osPriorityNormal, 1, 256);  // bmp280 height and temp
  UITask4Handle = osThreadCreate(osThread(UITask4), NULL);
  osThreadDef(UITask5, StartUITask5, osPriorityNormal, 1, 128); // flash onchip read write
  UITask5Handle = osThreadCreate(osThread(UITask5), NULL);
  osThreadDef(UITask6, StartUITask6, osPriorityNormal, 1, 128); // dht11 humidity
  UITask6Handle = osThreadCreate(osThread(UITask6), NULL);
  osThreadDef(UITask7, StartUITask7, osPriorityNormal, 1, 128); // uart console
  UITask7Handle = osThreadCreate(osThread(UITask7), NULL);
  osThreadDef(UITask8, StartUITask8, osPriorityNormal, 1, 128); // flowmeter read
  UITask8Handle = osThreadCreate(osThread(UITask8), NULL);
  osThreadDef(UITask9, StartUITask9, osPriorityNormal, 1, 256); // nuc read write
  UITask9Handle = osThreadCreate(osThread(UITask9), NULL);
  osThreadDef(UITask10, StartUITask10, osPriorityNormal, 1, 256); // uart2 modbus timer
  UITask10Handle = osThreadCreate(osThread(UITask10), NULL);
  osThreadDef(UITask11, StartUITask11, osPriorityNormal, 1, 128); // other board discover
  UITask11Handle = osThreadCreate(osThread(UITask11), NULL);
  osThreadDef(UITask12, StartUITask12, osPriorityNormal, 1, 256); // vl53l1x range
  UITask12Handle = osThreadCreate(osThread(UITask12), NULL);
  /* USER CODE END RTOS_THREADS */

  /* Start scheduler */
  osKernelStart();
  
  /* We should never get here as control is now taken by the scheduler */

  /* Infinite loop */
  /* USER CODE BEGIN WHILE */
  while (1)
  {
    /* USER CODE END WHILE */

    /* USER CODE BEGIN 3 */
  }
  /* USER CODE END 3 */
}

/**
  * @brief System Clock Configuration
  * @retval None
  */
void SystemClock_Config(void)
{
  RCC_OscInitTypeDef RCC_OscInitStruct = {0};
  RCC_ClkInitTypeDef RCC_ClkInitStruct = {0};

  /** Configure the main internal regulator output voltage 
  */
  __HAL_RCC_PWR_CLK_ENABLE();
  __HAL_PWR_VOLTAGESCALING_CONFIG(PWR_REGULATOR_VOLTAGE_SCALE1);
  /** Initializes the CPU, AHB and APB busses clocks 
  */
  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSE;
  RCC_OscInitStruct.HSEState = RCC_HSE_ON;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_ON;
  RCC_OscInitStruct.PLL.PLLSource = RCC_PLLSOURCE_HSE;
  RCC_OscInitStruct.PLL.PLLM = 4;
  RCC_OscInitStruct.PLL.PLLN = 144;
  RCC_OscInitStruct.PLL.PLLP = RCC_PLLP_DIV2;
  RCC_OscInitStruct.PLL.PLLQ = 4;
  if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
  {
    Error_Handler();
  }
  /** Initializes the CPU, AHB and APB busses clocks 
  */
  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1|RCC_CLOCKTYPE_PCLK2;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_PLLCLK;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV4;
  RCC_ClkInitStruct.APB2CLKDivider = RCC_HCLK_DIV2;

  if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_4) != HAL_OK)
  {
    Error_Handler();
  }
}

/**
  * @brief I2C2 Initialization Function
  * @param None
  * @retval None
  */
static void MX_I2C2_Init(void)
{

  /* USER CODE BEGIN I2C2_Init 0 */

  /* USER CODE END I2C2_Init 0 */

  /* USER CODE BEGIN I2C2_Init 1 */

  /* USER CODE END I2C2_Init 1 */
  hi2c2.Instance = I2C2;
  hi2c2.Init.ClockSpeed = 100000;
  hi2c2.Init.DutyCycle = I2C_DUTYCYCLE_2;
  hi2c2.Init.OwnAddress1 = 0;
  hi2c2.Init.AddressingMode = I2C_ADDRESSINGMODE_7BIT;
  hi2c2.Init.DualAddressMode = I2C_DUALADDRESS_DISABLE;
  hi2c2.Init.OwnAddress2 = 0;
  hi2c2.Init.GeneralCallMode = I2C_GENERALCALL_DISABLE;
  hi2c2.Init.NoStretchMode = I2C_NOSTRETCH_DISABLE;
  if (HAL_I2C_Init(&hi2c2) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN I2C2_Init 2 */

  /* USER CODE END I2C2_Init 2 */

}

/**
  * @brief TIM1 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM1_Init(void)
{

  /* USER CODE BEGIN TIM1_Init 0 */

  /* USER CODE END TIM1_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};

  /* USER CODE BEGIN TIM1_Init 1 */

  /* USER CODE END TIM1_Init 1 */
  htim1.Instance = TIM1;
  htim1.Init.Prescaler = 0;
  htim1.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim1.Init.Period = 65535;
  htim1.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim1.Init.RepetitionCounter = 0;
  htim1.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_Base_Init(&htim1) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_ETRMODE2;
  sClockSourceConfig.ClockPolarity = TIM_CLOCKPOLARITY_NONINVERTED;
  sClockSourceConfig.ClockPrescaler = TIM_CLOCKPRESCALER_DIV1;
  sClockSourceConfig.ClockFilter = 0;
  if (HAL_TIM_ConfigClockSource(&htim1, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim1, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM1_Init 2 */

  /* USER CODE END TIM1_Init 2 */

}

/**
  * @brief TIM2 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM2_Init(void)
{

  /* USER CODE BEGIN TIM2_Init 0 */

  /* USER CODE END TIM2_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};

  /* USER CODE BEGIN TIM2_Init 1 */

  /* USER CODE END TIM2_Init 1 */
  htim2.Instance = TIM2;
  htim2.Init.Prescaler = 0;
  htim2.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim2.Init.Period = 65535;
  htim2.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim2.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_Base_Init(&htim2) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_ETRMODE2;
  sClockSourceConfig.ClockPolarity = TIM_CLOCKPOLARITY_NONINVERTED;
  sClockSourceConfig.ClockPrescaler = TIM_CLOCKPRESCALER_DIV1;
  sClockSourceConfig.ClockFilter = 0;
  if (HAL_TIM_ConfigClockSource(&htim2, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim2, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM2_Init 2 */

  /* USER CODE END TIM2_Init 2 */

}

/**
  * @brief TIM3 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM3_Init(void)
{

  /* USER CODE BEGIN TIM3_Init 0 */

  /* USER CODE END TIM3_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};

  /* USER CODE BEGIN TIM3_Init 1 */

  /* USER CODE END TIM3_Init 1 */
  htim3.Instance = TIM3;
  htim3.Init.Prescaler = 0;
  htim3.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim3.Init.Period = 65535;
  htim3.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim3.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_Base_Init(&htim3) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_ETRMODE2;
  sClockSourceConfig.ClockPolarity = TIM_CLOCKPOLARITY_NONINVERTED;
  sClockSourceConfig.ClockPrescaler = TIM_CLOCKPRESCALER_DIV1;
  sClockSourceConfig.ClockFilter = 0;
  if (HAL_TIM_ConfigClockSource(&htim3, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim3, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM3_Init 2 */

  /* USER CODE END TIM3_Init 2 */

}

/**
  * @brief TIM4 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM4_Init(void)
{

  /* USER CODE BEGIN TIM4_Init 0 */

  /* USER CODE END TIM4_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};

  /* USER CODE BEGIN TIM4_Init 1 */

  /* USER CODE END TIM4_Init 1 */
  htim4.Instance = TIM4;
  htim4.Init.Prescaler = 0;
  htim4.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim4.Init.Period = 0;
  htim4.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim4.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_Base_Init(&htim4) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_ETRMODE2;
  sClockSourceConfig.ClockPolarity = TIM_CLOCKPOLARITY_NONINVERTED;
  sClockSourceConfig.ClockPrescaler = TIM_CLOCKPRESCALER_DIV1;
  sClockSourceConfig.ClockFilter = 0;
  if (HAL_TIM_ConfigClockSource(&htim4, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim4, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM4_Init 2 */

  /* USER CODE END TIM4_Init 2 */

}

/**
  * @brief USART1 Initialization Function
  * @param None
  * @retval None
  */
static void MX_USART1_UART_Init(void)
{

  /* USER CODE BEGIN USART1_Init 0 */

  /* USER CODE END USART1_Init 0 */

  /* USER CODE BEGIN USART1_Init 1 */

  /* USER CODE END USART1_Init 1 */
  huart1.Instance = USART1;
  huart1.Init.BaudRate = 115200;
  huart1.Init.WordLength = UART_WORDLENGTH_8B;
  huart1.Init.StopBits = UART_STOPBITS_1;
  huart1.Init.Parity = UART_PARITY_NONE;
  huart1.Init.Mode = UART_MODE_TX_RX;
  huart1.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart1.Init.OverSampling = UART_OVERSAMPLING_16;
  if (HAL_UART_Init(&huart1) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN USART1_Init 2 */

  /* USER CODE END USART1_Init 2 */

}

/**
  * @brief USART2 Initialization Function
  * @param None
  * @retval None
  */
static void MX_USART2_UART_Init(void)
{

  /* USER CODE BEGIN USART2_Init 0 */

  /* USER CODE END USART2_Init 0 */

  /* USER CODE BEGIN USART2_Init 1 */

  /* USER CODE END USART2_Init 1 */
  huart2.Instance = USART2;
  huart2.Init.BaudRate = 115200;
  huart2.Init.WordLength = UART_WORDLENGTH_8B;
  huart2.Init.StopBits = UART_STOPBITS_1;
  huart2.Init.Parity = UART_PARITY_NONE;
  huart2.Init.Mode = UART_MODE_TX_RX;
  huart2.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart2.Init.OverSampling = UART_OVERSAMPLING_16;
  if (HAL_UART_Init(&huart2) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN USART2_Init 2 */

  /* USER CODE END USART2_Init 2 */

}

/**
  * @brief USART3 Initialization Function
  * @param None
  * @retval None
  */
static void MX_USART3_UART_Init(void)
{

  /* USER CODE BEGIN USART3_Init 0 */

  /* USER CODE END USART3_Init 0 */

  /* USER CODE BEGIN USART3_Init 1 */

  /* USER CODE END USART3_Init 1 */
  huart3.Instance = USART3;
  huart3.Init.BaudRate = 115200;
  huart3.Init.WordLength = UART_WORDLENGTH_8B;
  huart3.Init.StopBits = UART_STOPBITS_1;
  huart3.Init.Parity = UART_PARITY_NONE;
  huart3.Init.Mode = UART_MODE_TX_RX;
  huart3.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart3.Init.OverSampling = UART_OVERSAMPLING_16;
  if (HAL_UART_Init(&huart3) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN USART3_Init 2 */

  /* USER CODE END USART3_Init 2 */

}

/**
  * @brief GPIO Initialization Function
  * @param None
  * @retval None
  */
static void MX_GPIO_Init(void)
{

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOC_CLK_ENABLE();
  __HAL_RCC_GPIOH_CLK_ENABLE();
  __HAL_RCC_GPIOA_CLK_ENABLE();
  __HAL_RCC_GPIOE_CLK_ENABLE();
  __HAL_RCC_GPIOB_CLK_ENABLE();
  __HAL_RCC_GPIOD_CLK_ENABLE();

}

/* USER CODE BEGIN 4 */
/*
int fputc(int ch,FILE *f)
{
    uint8_t temp[1]={ch};
    HAL_UART_Transmit(&huart1,temp,1,2);
}
*/

/* These next two functions converts the orientation matrix (see
 * gyro_orientation) to a scalar representation for use by the DMP.
 * NOTE: These functions are borrowed from Invensense's MPL.
 */
static  unsigned short inv_row_2_scale(const signed char *row)
{
    unsigned short b;

    if (row[0] > 0)
        b = 0;
    else if (row[0] < 0)
        b = 4;
    else if (row[1] > 0)
        b = 1;
    else if (row[1] < 0)
        b = 5;
    else if (row[2] > 0)
        b = 2;
    else if (row[2] < 0)
        b = 6;
    else
        b = 7;      // error
    return b;
}

static  unsigned short inv_orientation_matrix_to_scalar(const signed char *mtx)
{
    unsigned short scalar;

    /*
       XYZ  010_001_000 Identity Matrix
       XZY  001_010_000
       YXZ  010_000_001
       YZX  000_010_001
       ZXY  001_000_010
       ZYX  000_001_010
     */

    scalar = inv_row_2_scale(mtx);
    scalar |= inv_row_2_scale(mtx + 3) << 3;
    scalar |= inv_row_2_scale(mtx + 6) << 6;

    return scalar;
}

static void run_self_test(void)
{
    int result;

    long gyro[3], accel[3];

    result = mpu_run_self_test(gyro, accel);
    if (result == 0x7) 
    {
        /* Test passed. We can trust the gyro data here, so let's push it down
         * to the DMP.
         */
        float sens;
        unsigned short accel_sens;
        mpu_get_gyro_sens(&sens);
        gyro[0] = (long)(gyro[0] * sens);
        gyro[1] = (long)(gyro[1] * sens);
        gyro[2] = (long)(gyro[2] * sens);
        dmp_set_gyro_bias(gyro);
        mpu_get_accel_sens(&accel_sens);
        accel[0] *= accel_sens;
        accel[1] *= accel_sens;
        accel[2] *= accel_sens;
        dmp_set_accel_bias(accel);
				printf(" setting bias succesfully.\r\n");
    }
	else
	{
		printf(" bias has not been modified ...\r\n");
	}
    
}

/**
  * @brief GPIO_OV5640SCCB Initialization Function by user
  * @param None
  * @retval None
  */
/*
static void MX_GPIO_OV5640SCCB(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};

  __HAL_RCC_GPIOB_CLK_ENABLE();

  GPIO_InitStruct.Pin = GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
	
  GPIO_InitStruct.Pin = GPIO_PIN_10|GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

}
*/

/**
  * @brief GPIO_MPU9250IIC Initialization Function by user
  * @param None
  * @retval None
  */
static void MX_GPIO_MPU9250IIC(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};
	
  __HAL_RCC_GPIOB_CLK_ENABLE();

  GPIO_InitStruct.Pin = GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
	
  GPIO_InitStruct.Pin = GPIO_PIN_10|GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
}

/**
  * @brief GPIO_DHT11 Initialization Function by user
  * @param None
  * @retval None
  */
static void MX_GPIO_DHT11(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};
	
  __HAL_RCC_GPIOB_CLK_ENABLE();
	
  GPIO_InitStruct.Pin = GPIO_PIN_3;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
}

/**
  * @brief  Function implementing the UITask1 thread. DCMI communication with DVP camera OV5640.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask1(void const * argument)
{
	while(tftaskstarted==0)
	{
		osDelay(500);
	}
	
		printf("---------------------------------------\r\n");
	  printf(" camera ov5640 driver ignored.\r\n");
	  printf(" add files, modify and rebuild source if you need it.\r\n");
	  printf(" more info plz contact demotree.vip\r\n");
		osDelay(500);
	/*
	
		MX_GPIO_OV5640SCCB();
		for(int tryi=0;OV5640_Init();tryi++)
		{
			delay_ms(200);
			if(tryi>10)break;
		}
		ovx_mode=1;
		printf(" camera ov5640 initialization complete.\r\n");
		
		OV5640_RGB565_Mode();
		OV5640_Focus_Init();

		OV5640_JPEG_Mode();
		printf(" ov5640 jpeg mode ready.\r\n");
		
		OV5640_Light_Mode(0);
		OV5640_Color_Saturation(3);
		OV5640_Brightness(4);
		OV5640_Contrast(3);
		OV5640_Sharpness(33);
		OV5640_Focus_Constant();
		printf(" ov5640 focused.\r\n");
		
		OV5640_OutSize_Set(4,0,jpeg_img_size_tbl[size][0],jpeg_img_size_tbl[size][1]);
		HAL_DCMI_Start_DMA(&hdcmi, DCMI_MODE_SNAPSHOT, (uint32_t)&jpeg_buf,jpeg_buf_size);
		printf(" ov5640 snapshot test over.\r\n");
		
		OV5640_LED_light_0;
		
	*/
	
	camerataskstarted=1;
  /* Infinite loop */
  for(;1;)
  {
		taskYIELD();
		osDelay(1000);
  }

}

/**
  * @brief  Function implementing the UITask2 thread. Communication with TDK MEMS MPU9250.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask2(void const * argument)
{
	while(camerataskstarted==0)
	{
		osDelay(500);
	}
		printf("---------------------------------------\r\n");
	  printf(" acceleration posture sensor mpu9250 preparing.\r\n");
		osDelay(500);
	
		uint8_t result = 0;
		MX_GPIO_MPU9250IIC();
		result = mpu_init();//
		if(result)
		{
			printf(" mpu init failed.%d\r\n",result);
			delay_ms(500);
		}
		if(!result)   //
    {   
        //printf(" mems mpu9250 initialization complete.\r\n");
        
        //mpu_set_sensor
        if(!mpu_set_sensors(INV_XYZ_GYRO | INV_XYZ_ACCEL))
        {
            //printf(" mpu_set_sensor complete.\r\n");
        }
        else
        {
            printf(" mpu_set_sensor come across error\r\n");
        }
        
        //mpu_configure_fifo
        if(!mpu_configure_fifo(INV_XYZ_GYRO | INV_XYZ_ACCEL))
        {
            //printf(" mpu_configure_fifo complete.\r\n");
        }
        else
        {
            printf(" mpu_configure_fifo come across error\r\n");
        }
        
        //mpu_set_sample_rate
        if(!mpu_set_sample_rate(DEFAULT_MPU_HZ))
        {
            //printf(" mpu_set_sample_rate complete.\r\n");
        }
        else
        {
            printf(" mpu_set_sample_rate error\r\n");
        }
        
        //dmp_load_motion_driver_firmvare
				result = dmp_load_motion_driver_firmware();
        if(!result)
        {
            //printf(" dmp_load_motion_driver_firmware complete.\r\n");
        }
        else
        {
						printf(" dmp_load_motion_driver_firmware come across error %d\r\n",result);
        }
        
        //dmp_set_orientation
        if(!dmp_set_orientation(inv_orientation_matrix_to_scalar(gyro_orientation)))
        {
            //printf(" dmp_set_orientation complete.\r\n");
        }
        else
        {
            printf(" dmp_set_orientation come across error\r\n");
        }
        
        //dmp_enable_feature
        if(!dmp_enable_feature(DMP_FEATURE_6X_LP_QUAT | DMP_FEATURE_TAP |
            DMP_FEATURE_ANDROID_ORIENT | DMP_FEATURE_SEND_RAW_ACCEL | DMP_FEATURE_SEND_CAL_GYRO |
            DMP_FEATURE_GYRO_CAL))
        {
					//printf(" dmp_enable_feature complete.\r\n");
        }
        else
        {
					printf(" dmp_enable_feature come across error\r\n");
        }
        
        //dmp_set_fifo_rate
        if(!dmp_set_fifo_rate(DEFAULT_MPU_HZ))
        {
            //printf(" dmp_set_fifo_rate complete.\r\n");
        }
        else
        {
            printf(" dmp_set_fifo_rate come across error\r\n");
        }
        
        run_self_test();
        
        if(!mpu_set_dmp_state(1))
        {
            //printf(" mpu_set_dmp_state complete.\r\n");
        }
        else
        {
            printf(" mpu_set_dmp_state come across error\r\n");
        }
        
    }
		
		mpuinitresult=result;
		
  for(int i=0;i<1;i++)
  {
		//float Yaw,Roll,Pitch;
		while(dmp_read_fifo(gyro, accel, quat, &sensor_timestamp, &sensors, &more))
		{
			delay_us(1);
			if(result)break;
		}					

		if (sensors & INV_WXYZ_QUAT )
		{
			q0 = quat[0] / q30;
			q1 = quat[1] / q30;
			q2 = quat[2] / q30;
			q3 = quat[3] / q30;
			
			Pitch  = asin(-2 * q1 * q3 + 2 * q0* q2)* 57.3 + Pitch_error; // pitch
			Roll = atan2(2 * q2 * q3 + 2 * q0 * q1, -2 * q1 * q1 - 2 * q2* q2 + 1)* 57.3 + Roll_error; // roll
			Yaw = atan2(2*(q1*q2 + q0*q3),q0*q0+q1*q1-q2*q2-q3*q3) * 57.3 + Yaw_error;
			printf(" posture pitch:%.1f ",Pitch);
			printf("roll:%.1f  ",Roll);
			printf("yaw:%.1f   ",Yaw);
			printf("\r\n");
		}
		
		taskYIELD();
		osDelay(500);
  }
	
	memstaskstarted=1;
  /* Infinite loop */
	for(;1;)
  {
		/*
		while(dmp_read_fifo(gyro, accel, quat, &sensor_timestamp, &sensors, &more))
		{
			delay_us(1);
			if(result)break;
		}					

		if (sensors & INV_WXYZ_QUAT )
		{
			q0 = quat[0] / q30;
			q1 = quat[1] / q30;
			q2 = quat[2] / q30;
			q3 = quat[3] / q30;
			
			Pitch  = asin(-2 * q1 * q3 + 2 * q0* q2)* 57.3 + Pitch_error; // pitch
			Roll = atan2(2 * q2 * q3 + 2 * q0 * q1, -2 * q1 * q1 - 2 * q2* q2 + 1)* 57.3 + Roll_error; // roll
			Yaw = atan2(2*(q1*q2 + q0*q3),q0*q0+q1*q1-q2*q2-q3*q3) * 57.3 + Yaw_error;
		}
		*/
		taskYIELD();
		osDelay(5000);
  }
}

/**
  * @brief  Function implementing the UITask3 thread. exFat filesystem file open/read/write test on TF Card.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask3(void const * argument)
{
  
	while(defaulttaskover==0)
	{
		osDelay(500);
	}
		printf("---------------------------------------\r\n");
	  printf(" exfat usbdevice and tfcard driver ignored.\r\n");
	  printf(" add files, modify and rebuild source if you need it.\r\n");
	  printf(" more info plz contact demotree.vip\r\n");
	
		osDelay(500);	
	
	/*
		//-1- Mount in defaulttask     You should check defaulttask first

    //-2- Create and Open new text file objects with write access
    retSD = f_open(&fil, filename, FA_CREATE_ALWAYS | FA_WRITE);
    if(retSD)
		{
			printf(" open file error : %d\r\n",retSD);
		}
    else
		{
			printf(" open file for writing sucess. \r\n");
		}
		
    //-3- Write data to the text files
		pt = wtext1;
    retSD = f_write(&fil, pt, 32, (void *)&byteswritten);
		//retSD = f_write(&fil, "test", 4, (void *)&byteswritten);
    if(retSD)
		{
			printf(" write file error : %d\r\n",retSD);
		}
    else
    {
			printf(" write test sucess. \r\n");
    }
     
    //-4- Close the open text files
    retSD = f_close(&fil);
    if(retSD)
		{
			printf(" close error : %d\r\n",retSD);
		}
    else
		{
			printf(" close file sucess. \r\n");
		}
		
    //-5- Open the text files object with read access
    retSD = f_open(&fil, filename, FA_READ);
    if(retSD)
		{
			printf(" open file error : %d\r\n",retSD);
		}
    else
		{
			printf(" open file for reading sucess. \r\n");
		}
		
    //-6- Read data from the text files
    retSD = f_read(&fil, rtext1, sizeof(rtext1), (UINT*)&bytesread);
    if(retSD)
		{
			printf(" read error: %d\r\n",retSD);
		}
    else
    {
			printf(" read test sucess. \r\n");
			printf(" read data : %s\r\n",rtext1);
    }
     
    //-7- Close the open text files
    retSD = f_close(&fil);
    if(retSD)
		{
			printf(" close error: %d\r\n",retSD);
		}
    else
		{
			printf(" close file sucess. \r\n");
		}
		
    //-8- Compare read data with the expected data
    if(bytesread == byteswritten)
    { 
			printf(" exfat tfcard test over.\r\n");
    }
		
		//printf(" debugflag = %d\r\n",dbgi);

  */
	
	tftaskstarted=1;
	/* Infinite loop */
	for(;1;)
	{
		taskYIELD();
		osDelay(1500);
	}
}

/**
  * @brief  Function implementing the UITask4 thread. Communication with barometer BMP280.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask4(void const * argument)
{
  
	while(memstaskstarted==0)
	{
		osDelay(500);
	}
		printf("---------------------------------------\r\n");
		printf(" baro temperature sensor bmp280 preparing.\r\n");
		osDelay(500);	
	
		bmp280Init();
	
  for(int i=0;i<1;i++)
  {
    bmp280GetData(&bmp280_press,&bmp280_temp,&height);
		osDelay(500);
		pbmp=bmp280_press;
		tbmp=bmp280_temp;
		hbmp=height;
		printf(" barometric pressure:%d ",pbmp);
		printf("temperature:%d ",tbmp);
		printf("elevation:%d\r\n",hbmp);
		
		//bmp280GetRawData(&rawpressure,&rawtemperature);
		//printf(" bmp280_rawpress:%d ",rawpressure);
		//printf("bmp280_rawtemp:%d\r\n",rawtemperature);
		
		taskYIELD();
		osDelay(500);
  }
	
	bmptaskstarted=1;
	/* Infinite loop */
	for(;1;)
	{
		if(bmpdhtntf==1)
		{
			bmp280Init();
			bmp280GetData(&bmp280_press,&bmp280_temp,&height);
			osDelay(500);
			pbmp=bmp280_press;
			tbmp=bmp280_temp;
			hbmp=height;
			
			if(pbmp!=0||hbmp!=0)
			{
				baro=pbmp;
				elevation=hbmp;
				uint32_t curcnt;
//mod_send_bmp280:		
				curcnt = get_cyccnt();
				if(curcnt-modbuslaststamp>=get_gapcyccnt())
				{
					while( xSemaphoreTake( uart2sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
					mod_send_baro(&huart2,baro);
					osDelay(MOD_SILENT_MS*4);
					mod_send_elevation(&huart2,elevation);
					osDelay(MOD_SILENT_MS*4);
					mod_send_pitch(&huart2,Pitch);
					osDelay(MOD_SILENT_MS*4);
					mod_send_roll(&huart2,Roll);
					osDelay(MOD_SILENT_MS*4);
					mod_send_yaw(&huart2,Yaw);
					osDelay(MOD_SILENT_MS*4);
					xSemaphoreGive( uart2sph );
				}
				else
				{
					taskYIELD();
					//goto mod_send_bmp280;
				}
			}
		}
		
		taskYIELD();
		osDelay(500);
		
		srand(get_cyccnt());
    int rd = rand()%500 + 30;
		osDelay(rd);
	}
}

/**
  * @brief  Function implementing the UITask5 thread. Key-value storage on mcu itself. https://github.com/yanzi418/stm32_key_value under license MIT.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask5(void const * argument)
{
  
	while(bmptaskstarted==0)
	{
		osDelay(500);
	}
		printf("---------------------------------------\r\n");
		osDelay(500);	
		
		init_key_value2();
		reboot_times_history_info();
	
		/*
		char testwritein[8]={'t','e','s','t','1','2','3','\0'};
		uint8_t *ptw;
		uint32_t ptr;
		ptw = (uint8_t*)&testwritein[0];
		set_key_value( "teststr", STRINGS, ptw );
		get_key_value( "teststr", STRINGS, (uint8_t*)&ptr );
		printf( " test keyvalue string: key[teststr] value[%s]\r\n", (char*)ptr );
		*/
	
		get_key_value("kvcommkey", UINT32,(uint8_t *)(&kvcommkey));

		ptempstr = tempstr;
		sprintf(ptempstr,"%s"," keyvalue storage onchip initialized.\r\n");
		ptemp = (uint8_t*)ptempstr;
		HAL_UART_Transmit(&huart1, ptemp, strlen(ptempstr), 0x3ff);
		sprintf(ptempstr," board id: %s\r\n",pcpuidstr);
		ptemp = (uint8_t*)ptempstr;
		HAL_UART_Transmit(&huart1, ptemp, strlen(ptempstr), 0x3ff);
		
		get_key_value("modbusaddr", UINT32,(uint8_t *)(&tempaddr));
		if(tempaddr>0x00 && tempaddr<0xff)
		{
			modbusaddr = tempaddr;
			mbaddr = modbusaddr;
		}
			
	for(int i=0;i<1;i++)
	{

		taskYIELD();
		osDelay(500);
	}
	
	keyvaluetaskstarted=1;
	/* Infinite loop */
	for(;1;)
	{
		taskYIELD();
		osDelay(500);
	}
}


/**
  * @brief  Function implementing the UITask6 thread. Single IO data communication for cheap hygrometer DHT11.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask6(void const * argument)
{
  
	while(keyvaluetaskstarted==0)
	{
		osDelay(500);
	}
		printf("---------------------------------------\r\n");
	  printf(" temperature humidity sensor dht11 preparing.\r\n");
		osDelay(500);	
	
		MX_GPIO_DHT11();
		DHT11_Init();
		uint8_t ht = 0;
		uint8_t tp = 0;
		uint32_t raw = 0;
	
  for(int i=0;i<1;i++)
  {
		raw = getDHTRaw();
		ht=raw>>24;
		tp=(raw&0xffff)>>8;
		printf(" dht11 humidity: %d, temperature: %d \r\n",ht,tp);
		
		setiolow(90); //fix pb4 problem
		
		taskYIELD();
		osDelay(500);
  }
	
	dhttaskstarted=1;
	/* Infinite loop */
	for(;1;)
	{
		if(bmpdhtntf == 1)
		{
			raw = getDHTRaw();
			ht=raw>>24;
			tp=(raw&0xffff)>>8;
			
			taskYIELD();
			osDelay(500);
			
			srand(get_cyccnt());
      int rd = rand()%500 + 30;
			osDelay(rd);
			
			if(ht!=0||tp!=0)
			{
				temperature=tp;
				humidity=ht;
				uint32_t curcnt;
//mod_send_dht11:			
				curcnt = get_cyccnt();
				if(curcnt-modbuslaststamp>=get_gapcyccnt())
				{
					while( xSemaphoreTake( uart2sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
					mod_send_temperature(&huart2,temperature);
					osDelay(MOD_SILENT_MS*4);
					mod_send_humidity(&huart2,humidity);
					osDelay(MOD_SILENT_MS*4);
					xSemaphoreGive( uart2sph );
				}
				else
				{
					taskYIELD();
					//goto mod_send_dht11;
				}
				
			}
		}
		
		if(flowmeterntf == 1)
		{
			if(flowread1!=flowreadlast1 || flowread2!=flowreadlast2 || flowread3!=flowreadlast3 || flowread4!=flowreadlast4)
			{
				uint32_t curcnt;
//mod_send_pwm:
				curcnt = get_cyccnt();
				if(curcnt-modbuslaststamp>=get_gapcyccnt())
				{
					while( xSemaphoreTake( uart2sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
					mod_send_pwm1(&huart2,flowcount1);
					osDelay(MOD_SILENT_MS*4);
					mod_send_pwm2(&huart2,flowcount2);
					osDelay(MOD_SILENT_MS*4);
					mod_send_pwm3(&huart2,flowcount3);
					osDelay(MOD_SILENT_MS*4);
					mod_send_pwm4(&huart2,flowcount4);
					osDelay(MOD_SILENT_MS*4);
					xSemaphoreGive( uart2sph );
				}
				else
				{
					taskYIELD();
					//goto mod_send_pwm;
				}
				
			}
		}
		
		taskYIELD();
		osDelay(500);
	}
}

/**
  * @brief  Function implementing the UITask7 thread. UART1 console for user command input and result display. UART1 communication with other board.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask7(void const * argument)
{
  
	while(dhttaskstarted==0)
	{
		osDelay(500);
	}
		printf("---------------------------------------\r\n");
		osDelay(500);	
	
		if(HAL_UART_Receive_IT(&huart1,ptempdata,1) != HAL_OK)
		{
			HAL_UART_Transmit(&huart1, (uint8_t *)&" uart1 receiver setup error.\r\n",30,0x3ff);
			ptempdata=tempdata;		
		}
		else
		{
			ptempdata++;
			if(ptempdata>=tempdata+127)ptempdata=tempdata;
		}
		
		hst = HAL_UART_Receive_IT(&huart3,pmbdata,1);
		
		if(hst != HAL_OK)
		{
			HAL_UART_Transmit(&huart1, (uint8_t *)&" uart3 receiver setup error.\r\n",30,0x3ff);
			pmbdata=mbdata;		
		}
		else
		{
			pmbdata++;
			if(pmbdata>=mbdata+255)pmbdata=mbdata;
		}
		
		if(HAL_UART_Receive_IT(&huart2,pmodbusdata,1) != HAL_OK)
		{
			HAL_UART_Transmit(&huart1, (uint8_t *)&" uart2 receiver setup error.\r\n",30,0x3ff);
			pmodbusdata=modbusdata;		
		}
		else
		{
			pmodbusdata++;
			if(pmodbusdata>=modbusdata+255)pmodbusdata=modbusdata;
		}

		
		printf(" uart1 console started. \r\n");
	
  for(int i=0;i<1;i++)
  {
		
		taskYIELD();
		osDelay(500);
  }
	
	consoletaskstarted=1;
	/* Infinite loop */
	for(;1;)
	{
		if(displayflag==1) //console result display
		{
			ptemp = (uint8_t*)ptempstr;
			HAL_UART_Transmit(&huart1, ptemp, strlen(ptempstr),0x3ff);
			displayflag = 0;
		}
	
		taskYIELD();
		osDelay(500);
	}
}

/**
  * @brief  Function implementing the UITask8 thread. Square wave counter for flowmeter or other similar square wave sensors.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask8(void const * argument)
{
  
	while(consoletaskstarted==0)
	{
		osDelay(500);
	}
		printf("---------------------------------------\r\n");
		osDelay(500);	
	
		//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET); //waterpump relay
		//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET); //valve relay	
	
		flowbase1 = 0;
		flowreadlast1 = 0;
		flowread1 = 0;
		flowcount1 = 0;
		markrestart1 = 0;
		
		thistimestartflowcount1 = 0;
		tolerance1 = 0;
		thistimecount1 = 0;
		valvecontrol1 = 0;

		HAL_TIM_Base_Start(&htim1);
	
		flowbase2 = 0;
		flowreadlast2 = 0;
		flowread2 = 0;
		flowcount2 = 0;
		markrestart2 = 0;
		
		thistimestartflowcount2 = 0;
		tolerance2 = 0;
		thistimecount2 = 0;
		valvecontrol2 = 0;

		HAL_TIM_Base_Start(&htim2);
		
		flowbase3 = 0;
		flowreadlast3 = 0;
		flowread3 = 0;
		flowcount3 = 0;
		markrestart3 = 0;
		
		thistimestartflowcount3 = 0;
		tolerance3 = 0;
		thistimecount3 = 0;
		valvecontrol3 = 0;

		HAL_TIM_Base_Start(&htim3);
		
		flowbase4 = 0;
		flowreadlast4 = 0;
		flowread4 = 0;
		flowcount4 = 0;
		markrestart4 = 0;
		
		thistimestartflowcount4 = 0;
		tolerance4 = 0;
		thistimecount4 = 0;
		valvecontrol4 = 0;

		HAL_TIM_Base_Start(&htim4);
	
		printf(" flowmeters (square wave counters) started. \r\n");
	
  for(int i=0;i<1;i++)
  {
		
		taskYIELD();
		osDelay(500);
  }
	
	flowmetertaskstarted=1;
  /* Infinite loop */
  for(;1;)
  {
		//count increase
		flowread1 = (int32_t)htim1.Instance->CNT;
		if(abs(flowread1-flowreadlast1)>32768)
		{
			flowreadlast1 = flowreadlast1 - 65536;
			flowbase1+=65536;
			if(flowbase1>2100000000)
			{
				markrestart1 = 1;
			}
		}
		flowcount1 = flowbase1 + flowread1;
		flowreadlast1 = flowread1;
		
		flowread2 = (int32_t)htim2.Instance->CNT;
		if(abs(flowread2-flowreadlast2)>32768)
		{
			flowreadlast2 = flowreadlast2 - 65536;
			flowbase2+=65536;
			if(flowbase2>2100000000)
			{
				markrestart2 = 1;
			}
		}
		flowcount2 = flowbase2 + flowread2;
		flowreadlast2 = flowread2;
		
		flowread3 = (int32_t)htim3.Instance->CNT;
		if(abs(flowread3-flowreadlast3)>32768)
		{
			flowreadlast3 = flowreadlast3 - 65536;
			flowbase3+=65536;
			if(flowbase3>2100000000)
			{
				markrestart3 = 1;
			}
		}
		flowcount3 = flowbase3 + flowread3;
		flowreadlast3 = flowread3;
		
		flowread4 = (int32_t)htim4.Instance->CNT;
		if(abs(flowread4-flowreadlast4)>32768)
		{
			flowreadlast4 = flowreadlast4 - 65536;
			flowbase4+=65536;
			if(flowbase4>2100000000)
			{
				markrestart4 = 1;
			}
		}
		flowcount4 = flowbase4 + flowread4;
		flowreadlast4 = flowread4;
		
		//this time valve open count
		thistimecount1 = flowcount1 - thistimestartflowcount1;
		if(tolerance1>0 && thistimecount1>tolerance1)
		{
			tolerance1 = 0;
			thistimecount1 = 0;
			valvecontrol1 = 0; //close valve
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET);
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
			if(markrestart1==1)
			{
				__set_FAULTMASK(1);
				NVIC_SystemReset();
			}
		}
		if(tolerance1==0 && valvecontrol1==1)
		{
			tolerance1 = 0;
			thistimecount1 = 0;
			valvecontrol1 = 0; //close valve
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET);
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
		}
		
		thistimecount2 = flowcount2 - thistimestartflowcount2;
		if(tolerance2>0 && thistimecount2>tolerance2)
		{
			tolerance2 = 0;
			thistimecount2 = 0;
			valvecontrol2 = 0; //close valve
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET);
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
			if(markrestart2==1)
			{
				__set_FAULTMASK(1);
				NVIC_SystemReset();
			}
		}
		if(tolerance2==0 && valvecontrol2==1)
		{
			tolerance2 = 0;
			thistimecount2 = 0;
			valvecontrol2 = 0; //close valve
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET);
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
		}
		
		thistimecount3 = flowcount3 - thistimestartflowcount3;
		if(tolerance3>0 && thistimecount3>tolerance3)
		{
			tolerance3 = 0;
			thistimecount3 = 0;
			valvecontrol3 = 0; //close valve
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET);
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
			if(markrestart3==1)
			{
				__set_FAULTMASK(1);
				NVIC_SystemReset();
			}
		}
		if(tolerance3==0 && valvecontrol3==1)
		{
			tolerance3 = 0;
			thistimecount3 = 0;
			valvecontrol3 = 0; //close valve
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET);
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
		}
		
		thistimecount4 = flowcount4 - thistimestartflowcount4;
		if(tolerance4>0 && thistimecount4>tolerance4)
		{
			tolerance4 = 0;
			thistimecount4 = 0;
			valvecontrol4 = 0; //close valve
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET);
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
			if(markrestart4==1)
			{
				__set_FAULTMASK(1);
				NVIC_SystemReset();
			}
		}
		if(tolerance4==0 && valvecontrol4==1)
		{
			tolerance4 = 0;
			thistimecount4 = 0;
			valvecontrol4 = 0; //close valve
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET);
			//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
		}
		
		//
		
		//task delay
		taskYIELD();
    osDelay(100);
  }
}

/**
  * @brief  Function implementing the UITask9 thread. Intel usb2.0 communication.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask9(void const * argument)
{
  
	while(uart2taskstarted==0)
	{
		osDelay(500);
	}
		printf("---------------------------------------\r\n");
		osDelay(500);	
	
	/*	*/
		printf(" uart3 intel board communication preparing. \r\n");
		printf(" type \"help\" to get command manual url. \r\n");
		printf(" \r\n");
	
	get_key_value("kvcommkey", UINT32, (uint8_t *)(&commkey2));
	if(commkey==commkey2)
	{
		
	}
	else
	{
		//intelmodtaskstarted=1;
		skipflag = 1;
		printf(" ... \r\n");
	}
	
  for(int i=0;i<1;i++)
  {
		
		taskYIELD();
		osDelay(500);
  }
	

	
	intelmodtaskstarted=1;
  /* Infinite loop */
  for(;1;)
  {
		uint32_t curcnt2 = get_cyccnt();
		uint32_t gapcnt2 = get_gapcyccnt();
		if(curcnt2-mblaststamp>=gapcnt2 && mblastdealstamp != mblaststamp)
		{
			//deal data received
			ppmbdata = mdataf;
			uint8_t raddr2;
			uint8_t rfunc2;
			
			//while( xSemaphoreTake( modbusdatasph, ( TickType_t ) 10 ) != pdTRUE )osDelay(1);	
			if(skipflag==0)
			mod_recv(mbdata,mbtick,&raddr2,&rfunc2,&ppmbdata);
			//xSemaphoreGive( modbusdatasph );
			
			uint32_t* pir;
			uint32_t* pirdata;
			
			if(raddr2>=MOD_ADDR && raddr2<=MOD_ADDR2 && raddr2 != mbaddr) //command data from intel board
			{
				if(rfunc2==REGREAD)
				{
					pir=(uint32_t*)&ppmbdata[0];
					sendotherdata2u3(raddr2,*pir,gettargetdata(raddr2,*pir));
				}
				else if(rfunc2==COILREAD)
				{
					pir=(uint32_t*)&ppmbdata[0];
					sendotheriostate2u3(raddr2,(uint16_t)*pir,gettargetiostate(raddr2,(uint16_t)*pir));		
				}
				else if(rfunc2==COILWRITE)
				{
					pir=(uint32_t*)&ppmbdata[0];
					pirdata=(uint32_t*)&ppmbdata[4];
					writeothertargetcoil2u2(raddr2,(uint16_t)*pir,(uint16_t)*pirdata);
					sendotheriostate2u3(raddr2,(uint16_t)*pir,getwritebackiostate(raddr2,(uint16_t)*pir));		
				}
		  }
			else if(raddr2 == mbaddr) //command to this board
			{
					if(rfunc2==REGREAD)
					{
						pir=(uint32_t*)&ppmbdata[0];
						sendselfdata2u3(*pir);
					}
					else if(rfunc2==REGWRITE)
					{
						pir=(uint32_t*)&ppmbdata[0];
						sendselfdata2u3(*pir);
						if(*pir==WMOTORFWD)
						{
							pirdata=(uint32_t*)&ppmbdata[4];
							uint32_t mid = *pirdata;
							pirdata=(uint32_t*)&ppmbdata[8];
							uint32_t stp = *pirdata;
							motor42_forward(mid,stp);
						}
						else if(*pir==WMOTORRVS)
						{
							pirdata=(uint32_t*)&ppmbdata[4];
							uint32_t mid = *pirdata;
							pirdata=(uint32_t*)&ppmbdata[8];
							uint32_t stp = *pirdata;
							motor42_reverse(mid,stp);
						}
						else if(*pir==WMOTORINIT)
						{
							pirdata=(uint32_t*)&ppmbdata[4];
							uint32_t mid = *pirdata;
							pirdata=(uint32_t*)&ppmbdata[8];
							uint32_t grb = *pirdata;
							pirdata=(uint32_t*)&ppmbdata[12];
							uint32_t pnm = *pirdata;
							motor_initpin(mid,grb,pnm);
						}
					}
					else if(rfunc2==COILREAD)
					{
						pir=(uint32_t*)&ppmbdata[0];
						sendselfiostate2u3((uint16_t)*pir);
					}
					else if(rfunc2==COILWRITE)
					{	
						pir=(uint32_t*)&ppmbdata[0];
						pirdata=(uint32_t*)&ppmbdata[4];
						if(*pir==0xfc)
						{
							//force reboot pinnum==252
							__set_FAULTMASK(1);
							NVIC_SystemReset();
						}
						
						if(*pirdata==0)
						{
							setiolow((uint16_t)*pir);
						}
						else if(*pirdata==1)
						{
							setiohigh((uint16_t)*pir);
						}
						else if(*pirdata==2)
						{
							setioout((uint16_t)*pir);
						}
						else if(*pirdata==4)
						{
							setioin((uint16_t)*pir);
						}
						sendselfiostate2u3((uint16_t)*pir);
					}
			}
			
			//while( xSemaphoreTake( modbusdatasph, ( TickType_t ) 10 ) != pdTRUE )osDelay(1);				
			mbtick = 0;
			pmbdata = mbdata;
			memset(pmbdata,0,256);
			pmbdata = mbdata;
			//xSemaphoreGive( modbusdatasph );
			
			mblastdealstamp = mblaststamp;
		}
		taskYIELD();
		osDelay(1);
		

	}
}

/**
  * @brief  Function implementing the UITask10 thread. UART2 modbus timer.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask10(void const * argument)
{
	while(vl53taskstarted==0)
	{
		osDelay(500);
	}
	/*	*/
		printf("---------------------------------------\r\n");
		osDelay(500);	
	
		printf(" uart2 modbus timer and vfd modbus controllor prepared. \r\n");
	
		sprintf(ptempstr," board modbus address: 0x%2x\r\n",modbusaddr);
		ptemp = (uint8_t*)ptempstr;
		HAL_UART_Transmit(&huart1, ptemp, strlen(ptempstr), 0x3ff);
	
	get_key_value("kvcommkey", UINT32, (uint8_t *)(&commkey2));
	if(commkey==commkey2)
	{
		
	}
	else
	{
		//uart2taskstarted=1;
		skipflag = 1;
		printf(" ... \r\n");
	}
	
  for(int i=0;i<1;i++)
  {
		
		taskYIELD();
		osDelay(500);
  }

	uart2taskstarted=1;
  /* Infinite loop */
	
  for(;1;)
  {
		uint32_t curcnt = get_cyccnt();
		uint32_t gapcnt = get_gapcyccnt();
		if(curcnt-modbuslaststamp>=gapcnt && lastdealstamp != modbuslaststamp)
		{
			//deal data received
			ppmdata = pmdata;
			uint8_t raddr;
			uint8_t rfunc;
			
			//while( xSemaphoreTake( modbusdatasph, ( TickType_t ) 10 ) != pdTRUE )osDelay(1);	
			if(skipflag==0)
			mod_recv(modbusdata,modbustick,&raddr,&rfunc,&ppmdata);
			//xSemaphoreGive( modbusdatasph );
			
			uint32_t* pir;
			uint32_t* pirdata;
			float* firdata;	
			
			if(raddr>=MOD_ADDR && raddr<=MOD_ADDR2 && raddr != modbusaddr) //sensors data from other Thylacine board
			{
				int k = -1;
				for(int j=0;j<16;j++)
				{
					if(otherboarddata[j][254] == 0 || otherboarddata[j][254] == raddr)
					{
						k = j;
						break;
					}
				}
				if(k==-1)
				{
					 printf(" sensor data too much. \r\n");
					 /*
					 for(int m=0;m<16;m++)
					 {
						 memset(otherboarddata[m],0,255);
					 }
					 */
					 osDelay(1000);
				}
				else
				{				
					if(rfunc==REGREAD)
					{
						pir=(uint32_t*)&ppmdata[0];
						if((*pir)<254)
						{
							if((*pir)==RPITCH||(*pir)==RROLL||(*pir)==RYAW)
							{
								firdata=(float*)&ppmdata[4];
								pirdata=(uint32_t*)&ppmdata[4];
								if(*pirdata!=0x55aa55aa)
								{
									otherboarddata[k][254]=raddr;
									otherboarddata[k][(*pir)]=*firdata;	
								}
							}
							else
							{
								pirdata=(uint32_t*)&ppmdata[4];
								if(*pirdata!=0x55aa55aa)
								{
									otherboarddata[k][254]=raddr;
									otherboarddata[k][(*pir)]=*pirdata;						
								}		
							}
					
						}
						readingflag = 0;
					}
					else if(rfunc==COILREAD)
					{
						if((uint32_t)ppmdata[4]!=0x55aa55aa)
						{
							otherboarddata[k][254]=raddr;
							pir=(uint32_t*)&ppmdata[0];
							pirdata=(uint32_t*)&ppmdata[4];
							otherboarddata[k][CLASTPINNUM]=*pir;
							otherboarddata[k][CSTATE]=*pirdata;
						}
						readingflag = 0;
					}
					
				}
		  }
			else if(raddr == modbusaddr) //command to this board
			{
					if(rfunc==COILREAD)
					{
						pir=(uint32_t*)&ppmdata[0];
						sendselfiostate2u2(*pir);
					}
					else if(rfunc==COILWRITE)
					{
						pir=(uint32_t*)&ppmdata[0];
						pirdata=(uint32_t*)&ppmdata[4];
						if(*pir==0xfc)
						{
							//force reboot pinnum==252
							__set_FAULTMASK(1);
							NVIC_SystemReset();
						}
						
						if(*pirdata==0)
						{
							setiolow((uint16_t)*pir);
						}
						else if(*pirdata==1)
						{
							setiohigh((uint16_t)*pir);
						}
						else if(*pirdata==2)
						{
							setioout((uint16_t)*pir);
						}
						else if(*pirdata==4)
						{
							setioin((uint16_t)*pir);
						}
						
						sendselfiostate2u2((uint16_t)*pir);
						
					}
					else if(rfunc==REGREAD)
					{
						pir=(uint32_t*)&ppmdata[0];
						pirdata=(uint32_t*)&ppmdata[4];
						if(*pirdata==0x55aa55aa)
						{
							sendselfdata2u2(*pir);
						}
						else
						{
							//printf("unknown modbus read command.");
						}
					}
			}
			
			if(raddr==HITACHI_WJ200) //VFD data from HITACHI WJ200
			{
				if(rfunc==REGREAD)
				{

				}
			}
			
			//while( xSemaphoreTake( modbusdatasph, ( TickType_t ) 10 ) != pdTRUE )osDelay(1);				
			modbustick = 0;
			pmodbusdata = modbusdata;
			memset(pmodbusdata,0,256);
			pmodbusdata = modbusdata;
			//xSemaphoreGive( modbusdatasph );
			
			lastdealstamp = modbuslaststamp;
		}
		taskYIELD();
		osDelay(1);
	}

}

/**
  * @brief  Function implementing the UITask11 thread. other board discover.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask11(void const * argument)
{
	while(intelmodtaskstarted==0)
	{
		osDelay(500);
	}
	
	//set default pins
	
	
	discovertaskstarted = 1;
	
  /* Infinite loop */
	
  for(;1;)
  {
		if(discoverflag==1)
		{
				osDelay(2000);
			
				for(int m=MOD_ADDR;m<=MOD_ADDR2;m++)
				{
					for(int n=1;n<SENSORMAX;n++)
					{
						readothertargetdata2u2(m,n);
						printf(".");
						taskYIELD();
						osDelay(1000);
					}
				}
				discoverflag = 0;
				printf("\r\n discover complete.\r\n");
		}
		
		refreshmpu9250();
		osDelay(100);
		refreshbmp280();
		osDelay(100);
		refreshdht11();
		osDelay(100);
		refreshvl53l1x();
		osDelay(100);
		
		taskYIELD();
		osDelay(1600);
	}
}

/**
  * @brief  Function implementing the UITask12 thread. vl53l1x range.
  * @param  argument: Not used 
  * @retval None
  */
void StartUITask12(void const * argument)
{
	while(flowmetertaskstarted==0)
	{
		osDelay(500);
	}
	/*	*/
		printf("---------------------------------------\r\n");
		osDelay(500);	
	
		int status = 0;
    VL53L1_RangingMeasurementData_t md;
    uint8_t dataReady = 0;
	
    //uint8_t byteData;
    //uint16_t wordData;		
		//uint8_t sensorState=0;	
	
		dev53.I2cDevAddr = VL53ADDR;
		dev53.comms_speed_khz = 100;
		dev53.comms_type = 1;
		dev53.I2cHandle = &hi2c2;
		dev53.new_data_ready_poll_duration_ms = 35;
	  //dev53.Data = dt53;
#ifdef V53USEHALI2C
		//MX_I2C2_Init();
#endif
	
		//XSHUT
		/*
		setioout(29);
		setiolow(29);
		osDelay(30);
		setiohigh(29);
		osDelay(30);
		*/
		
#ifndef V53USEHALI2C
		IIC_Init();
#endif

    /* Initialize and configure the device according to people counting need */
		/*
		status = VL53L1_WaitDeviceBooted(&dev53);
		if (status != 0) 
		{
       printf(" error in booting the vl53l1x %d\r\n",status);
    }
		*/
		
		status = VL53L1_DataInit(&dev53);
		if (status != 0) 
		{
        printf(" error datainit %d\r\n",status);
    }
    status = VL53L1_StaticInit(&dev53);
		if (status != 0) 
		{
        printf(" error staticinit %d\r\n",status);
    }
    //status = VL53L1_PerformRefSpadManagement(&dev53);
		//if (status != 0) 
		//{
    //   printf(" error refspad %d\r\n",status);
    //}
    status = VL53L1_SetDistanceMode(&dev53, VL53L1_DISTANCEMODE_SHORT); 
		if (status != 0) 
		{
        printf(" error setdistancemode %d\r\n",status);
    }
    status = VL53L1_SetMeasurementTimingBudgetMicroSeconds (&dev53, 30000); 
		if (status != 0) 
		{
        printf(" error settimingbudget %d\r\n",status);
    }
    status = VL53L1_SetInterMeasurementPeriodMilliSeconds (&dev53, 500);
    if (status != 0) 
		{
        printf(" error setmeasurementperiod %d\r\n",status);
    }
		printf(" vl53l1x range sensor prepared. \r\n");

	
		for(int i=0;i<1;i++)
		{
			status = VL53L1_StartMeasurement(&dev53);
			if (status != 0) 
			{
				printf(" error in starting the vl53l1x %d\r\n",status);
			}
			int j=0;
			for(j=0;j<128&&dataReady == 0;j++)
			{
				status = VL53L1_GetMeasurementDataReady(&dev53, &dataReady);
				osDelay(10);
			}
			dataReady = 0;
			if(j==128)
			{
				printf(" data not ready\r\n");
			}
			status = VL53L1_GetRangingMeasurementData(&dev53, &md);
			if (status != 0) 
			{
				printf(" error in operating the vl53l1x %d\r\n",status);
			}
			status = VL53L1_ClearInterruptAndStartMeasurement (&dev53); 
			if (status != 0) 
			{
				printf(" error in clearint the vl53l1x %d\r\n",status);
			}
			if(md.RangeStatus==0)
			{
				distance = md.RangeMilliMeter;
				printf(" distence: %dmm\r\n",md.RangeMilliMeter);
			}
			else
			{
				printf(" status: %d, distence: %dmm\r\n",md.RangeStatus,md.RangeMilliMeter);
			}
			/*
			status = VL53L1_StopMeasurement(&dev53);
			if (status != 0) 
			{
				printf(" error in stoping the vl53l1x %d\r\n",status);
			}
			*/
			
			//taskYIELD();
			//osDelay(500);
		}
		/*
		status = VL53L1_StartMeasurement(&dev53);
		if (status != 0) 
		{
			printf(" error in starting measurement\n");
		}
		*/
#ifdef V53USEHALI2C
		//MX_GPIO_MPU9250IIC();
#endif	
	vl53taskstarted = 1;
	
  /* Infinite loop */
  for(;1;)
  {
		/*		*/
		if(refreshvl53l1xflag==1)
		{
			while (dataReady == 0)
			{
				status = VL53L1_GetMeasurementDataReady(&dev53, &dataReady);
				osDelay(10);
			}
			dataReady = 0;
			status = VL53L1_GetRangingMeasurementData(&dev53, &md);
			status += VL53L1_ClearInterruptAndStartMeasurement (&dev53); 
			if (status != 0) 
			{
				//printf(" error in operating the vl53l1x\n");
			}
			if(md.RangeStatus==0)
			{
				distance = md.RangeMilliMeter;
				//printf(" distence: %dmm\n",md.RangeMilliMeter);
			}	
			refreshvl53l1xflag = 0;
		}

		taskYIELD();
		osDelay(1000);
	}
}

/**
  * @brief  refresh vl53l1x data.
  * @param  argument: void
  * @retval void
  */
void refreshvl53l1x(void)
{
	refreshvl53l1xflag = 1;
	/*
		int status = 0;
    uint8_t dataReady = 0;
	
		for(int i=0;i<1;i++)
		{
			int j=0;
			for(j=0;j<128&&dataReady == 0;j++)
			{
				status = VL53L1_GetMeasurementDataReady(&dev53, &dataReady);
				osDelay(10);
			}
			dataReady = 0;
			if(j==128)
			{
				//printf(" data not ready\r\n");
			}
			status = VL53L1_GetRangingMeasurementData(&dev53, &mdg);
			if (status != 0) 
			{
				//printf(" error in operating the vl53l1x %d\r\n",status);
			}
			status = VL53L1_ClearInterruptAndStartMeasurement (&dev53); 
			if (status != 0) 
			{
				//printf(" error in clearint the vl53l1x %d\r\n",status);
			}
			if(mdg.RangeStatus==0)
			{
				distance = mdg.RangeMilliMeter;
				//printf(" distence: %dmm\r\n",mdg.RangeMilliMeter);
			}
			else
			{
				//printf(" status: %d, distence: %dmm\r\n",mdg.RangeStatus,mdg.RangeMilliMeter);
			}
			
			
			//taskYIELD();
			//osDelay(500);
		}
		
	*/
}

/**
  * @brief  refresh mpu9250 data.
  * @param  argument: void
  * @retval void
  */
void refreshmpu9250(void)
{
					for(int k=0;dmp_read_fifo(gyro, accel, quat, &sensor_timestamp, &sensors, &more)&&k<1024;k++)
					{
						delay_us(1);
						if(mpuinitresult)break;
					}					

					if (sensors & INV_WXYZ_QUAT )
					{
						q0 = quat[0] / q30;
						q1 = quat[1] / q30;
						q2 = quat[2] / q30;
						q3 = quat[3] / q30;
						
						Pitch  = asin(-2 * q1 * q3 + 2 * q0* q2)* 57.3 + Pitch_error; // pitch
						Roll = atan2(2 * q2 * q3 + 2 * q0 * q1, -2 * q1 * q1 - 2 * q2* q2 + 1)* 57.3 + Roll_error; // roll
						Yaw = atan2(2*(q1*q2 + q0*q3),q0*q0+q1*q1-q2*q2-q3*q3) * 57.3 + Yaw_error;
					}
}

/**
  * @brief  refresh bmp280 data.
  * @param  argument: void
  * @retval void
  */
void refreshbmp280(void)
{
			bmp280Init();
			bmp280GetData(&bmp280_press,&bmp280_temp,&height);
			osDelay(500);
			pbmp=bmp280_press;
			tbmp=bmp280_temp;
			hbmp=height;
			
			if(pbmp!=0||hbmp!=0)
			{
				baro=pbmp;
				elevation=hbmp;
			}
}

/**
  * @brief  refresh dht11 data.
  * @param  argument: void
  * @retval void
  */
void refreshdht11(void)
{
			uint32_t raw = getDHTRaw();
			uint32_t ht=raw>>24;
			uint32_t tp=(raw&0xffff)>>8;
			
			if(ht!=0||tp!=0)
			{
				temperature=tp;
				humidity=ht;
			}
}


/**
  * @brief  get target board write back io state (state after write operation).
  * @param  argument: modaddr pinnum
  * @retval sensor data
  */
uint8_t getwritebackiostate(uint32_t modaddr,uint16_t pinnum)
{
	if(modaddr==modbusaddr)
	{
		return getiostate(pinnum);
	}
	else
	{
		int i,m;
		for(i=0;i<10;i++)
		{
			for(m=0;readingflag==1&&m<32;m++)osDelay(100);
			if(m>=32)
			{
				continue;
			}
			else
			{
				break;
			}
		}
		if(i>=10)return 0xfd;
	}
	
				int k = -1;
				for(int j=0;j<16;j++)
				{
					if(otherboarddata[j][254] == modaddr)
					{
						k = j;
						break;
					}
				}
				if(k==-1)
				{
					 return 0xfe;
				}
				else
				{
					if(((uint16_t)otherboarddata[k][CLASTPINNUM])==pinnum)
					{
						return otherboarddata[k][CSTATE];
					}
					else
					{
						return 0xff;
					}
				}
}

/**
  * @brief  get target board io state.
  * @param  argument: modaddr pinnum
  * @retval sensor data
  */
uint8_t gettargetiostate(uint32_t modaddr,uint16_t pinnum)
{
	if(modaddr==modbusaddr)
	{
		return getiostate(pinnum);
	}
	else
	{
		int i,m;
		for(i=0;i<10;i++)
		{
			readothertargetcoil2u2(modaddr,pinnum);
			for(m=0;readingflag==1&&m<32;m++)osDelay(100);
			if(m>=32)
			{
				continue;
			}
			else
			{
				break;
			}
		}
	}
	
				int k = -1;
				for(int j=0;j<16;j++)
				{
					if(otherboarddata[j][254] == modaddr)
					{
						k = j;
						break;
					}
				}
				if(k==-1)
				{
					 return 0xfe;
				}
				else
				{
					if(((uint16_t)otherboarddata[k][CLASTPINNUM])==pinnum)
					{
						return otherboarddata[k][CSTATE];
					}
					else
					{
						return 0xff;
					}
				}
}

/**
  * @brief  get target board sensors data.
  * @param  argument: modaddr sensorreg
  * @retval sensor data
  */
uint32_t gettargetdata(uint32_t modaddr,uint32_t sensorreg)
{
	if(modaddr==modbusaddr)
	{
					if(sensorreg==RBARO)
					{
						//refreshbmp280();
						return baro;
					}
					else if(sensorreg==RELEVATION)
					{
						//refreshbmp280();
						return elevation;
					}
					else if(sensorreg==RTEMPERATURE)
					{
						//refreshdht11();
						return temperature;
					}
					else if(sensorreg==RHUMIDITY)
					{
						//refreshdht11();
						return humidity;
					}
					else if(sensorreg==RPWM1)
					{
						return flowcount1;
					}
					else if(sensorreg==RPWM2)
					{
						return flowcount2;
					}
					else if(sensorreg==RPWM3)
					{
						return flowcount3;
					}
					else if(sensorreg==RPWM4)
					{
						return flowcount4;
					}
					else if(sensorreg==RPITCH)
					{
						//refreshmpu9250();
						return *(uint32_t*)&Pitch;
					}
					else if(sensorreg==RROLL)
					{
						//refreshmpu9250();
						return *(uint32_t*)&Roll;
					}
					else if(sensorreg==RYAW)
					{
						//refreshmpu9250();
						return *(uint32_t*)&Yaw;
					}
					else if(sensorreg==WMOTORFWD)
					{
						return 0;
					}
					else if(sensorreg==WMOTORRVS)
					{
						return 0;
					}
					else if(sensorreg==WMOTORINIT)
					{
						return 0;
					}
					else if(sensorreg==RDISTANCE)
					{
						//refreshvl53l1x();
						return distance;
					}
	}
	
	int i,m;
	for(i=0;i<10;i++)
	{
		readothertargetdata2u2(modaddr,sensorreg);
		for(m=0;readingflag==1&&m<32;m++)osDelay(100);
		if(m>=32)
		{
			continue;
		}
		else
		{
			break;
		}
	}
	
				int k = -1;
				for(int j=0;j<16;j++)
				{
					if(otherboarddata[j][254] == modaddr)
					{
						k = j;
						break;
					}
				}
				if(k==-1)
				{
					 return 0xffff;
				}
				else
				{
					return otherboarddata[k][sensorreg];
				}
}

/**
  * @brief  start send self sensors data.
  * @param  argument: sensorreg
  * @retval void
  */
void sendselfdata2u2(uint32_t sensorreg)
{
		osDelay(MOD_SILENT_MS*4);
	
		//uint32_t curcnt = get_cyccnt();
		//while(curcnt-modbuslaststamp<get_gapcyccnt()){osDelay(10);curcnt = get_cyccnt();}
		osDelay(MOD_SILENT_MS);

		while( xSemaphoreTake( uart2sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_getotherreg(&huart2,modbusaddr,sensorreg,gettargetdata(modbusaddr,sensorreg));
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart2sph );
}

/**
  * @brief  start send self sensors data.
  * @param  argument: sensorreg
  * @retval void
  */
void sendselfdata2u3(uint32_t sensorreg)
{
		osDelay(MOD_SILENT_MS*4);
	
		//uint32_t curcnt2 = get_cyccnt();
		//while(curcnt2-mblaststamp<get_gapcyccnt()){osDelay(10);curcnt2 = get_cyccnt();}
		osDelay(MOD_SILENT_MS);
	
		while( xSemaphoreTake( uart3sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_getotherreg(&huart3,modbusaddr,sensorreg,gettargetdata(modbusaddr,sensorreg));
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart3sph );
}

/**
  * @brief  start send self coil state.
  * @param  argument: pinnum
  * @retval void
  */
void sendselfiostate2u2(uint16_t pinnum)
{
		osDelay(MOD_SILENT_MS*4);
	
		//uint32_t curcnt = get_cyccnt();
		//while(curcnt-modbuslaststamp<get_gapcyccnt()){osDelay(10);curcnt = get_cyccnt();}
		osDelay(MOD_SILENT_MS);
	
		while( xSemaphoreTake( uart2sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_coilstate(&huart2,pinnum,getiostate(pinnum));
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart2sph );
}

/**
  * @brief  start send self coil state.
  * @param  argument: pinnum
  * @retval void
  */
void sendselfiostate2u3(uint16_t pinnum)
{
		osDelay(MOD_SILENT_MS*4);
	
		//uint32_t curcnt2 = get_cyccnt();
		//while(curcnt2-mblaststamp<get_gapcyccnt()){osDelay(10);curcnt2 = get_cyccnt();}
		osDelay(MOD_SILENT_MS);
	
		while( xSemaphoreTake( uart3sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_coilstate(&huart3,pinnum,getiostate(pinnum));
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart3sph );
}

/**
  * @brief  start send reg data to intel board.
  * @param  argument: regaddr sensordata
  * @retval void
  */
void sendotherdata2u3(uint32_t modaddr,uint32_t regaddr,uint32_t sensordata)
{
		//uint32_t curcnt2 = get_cyccnt();
		//while(curcnt2-mblaststamp<get_gapcyccnt()){osDelay(10);curcnt2 = get_cyccnt();}
		osDelay(MOD_SILENT_MS);
	
		while( xSemaphoreTake( uart3sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_getotherreg(&huart3,modaddr,regaddr,sensordata);
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart3sph );
}

/**
  * @brief  start send other board coil state to intel board.
  * @param  argument: pinnum
  * @retval void
  */
void sendotheriostate2u3(uint32_t modaddr,uint16_t pinnum,uint16_t pinstate)
{
		//uint32_t curcnt2 = get_cyccnt();
		//while(curcnt2-mblaststamp<get_gapcyccnt()){osDelay(10);curcnt2 = get_cyccnt();}
		osDelay(MOD_SILENT_MS);
	
		while( xSemaphoreTake( uart3sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_othercoilstate(&huart3,modaddr,pinnum,pinstate);
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart3sph );
}


/**
  * @brief  start read other board sensors data.
  * @param  argument: modaddr sensorreg
  * @retval void
  */
void readothertargetdata2u2(uint32_t modaddr,uint32_t sensorreg)
{
	readingflag = 1;
		//uint32_t curcnt = get_cyccnt();
		//while(curcnt-modbuslaststamp<get_gapcyccnt()){osDelay(10);curcnt = get_cyccnt();}
		osDelay(MOD_SILENT_MS);
	
		while( xSemaphoreTake( uart2sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_readotherreg(&huart2,modaddr,sensorreg);
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart2sph );

}

/**
  * @brief  start read other board coil state.
  * @param  argument: modaddr pinnum
  * @retval void
  */
void readothertargetcoil2u2(uint32_t modaddr,uint16_t pinnum)
{
	readingflag = 1;
		//uint32_t curcnt = get_cyccnt();
		//while(curcnt-modbuslaststamp<get_gapcyccnt()){osDelay(10);curcnt = get_cyccnt();}
		osDelay(MOD_SILENT_MS);
	
		while( xSemaphoreTake( uart2sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_readothercoil(&huart2,modaddr,pinnum);
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart2sph );

}

/**
  * @brief  start write other board coil.
  * @param  argument: modaddr pinnum
  * @retval void
  */
void writeothertargetcoil2u2(uint32_t modaddr,uint16_t pinnum,uint16_t pinstate)
{
	readingflag = 1;
		//uint32_t curcnt = get_cyccnt();
		//while(curcnt-modbuslaststamp<get_gapcyccnt()){osDelay(10);curcnt = get_cyccnt();}
		osDelay(MOD_SILENT_MS);
	
		while( xSemaphoreTake( uart2sph, ( TickType_t ) 10 ) != pdTRUE )osDelay(MOD_SILENT_MS);
		mod_send_writeothercoil(&huart2,modaddr,pinnum,pinstate);
		osDelay(MOD_SILENT_MS*4);
		xSemaphoreGive( uart2sph );

}

/**
  * @brief  Set a tolerance number and set the valve and flowmeter on.
  * @param  argument: tolerance
  * @retval None
  */
void setTolerance1(uint32_t tl)
{
	tolerance1 = tl; //set tolerance
	thistimestartflowcount1 = flowcount1; //remember current flowcount
	thistimecount1 = 0; //start count this time
	valvecontrol1 = 1; //open valve
	//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_SET);
	//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_SET);
}
void setTolerance2(uint32_t tl)
{
	tolerance2 = tl; //set tolerance
	thistimestartflowcount2 = flowcount2; //remember current flowcount
	thistimecount2 = 0; //start count this time
	valvecontrol2 = 1; //open valve
	//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_SET);
	//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_SET);
}
void setTolerance3(uint32_t tl)
{
	tolerance3 = tl; //set tolerance
	thistimestartflowcount3 = flowcount3; //remember current flowcount
	thistimecount3 = 0; //start count this time
	valvecontrol3 = 1; //open valve
	//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_SET);
	//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_SET);
}
void setTolerance4(uint32_t tl)
{
	tolerance4 = tl; //set tolerance
	thistimestartflowcount4 = flowcount4; //remember current flowcount
	thistimecount4 = 0; //start count this time
	valvecontrol4 = 1; //open valve
	//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_SET);
	//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_SET);
}



/**
  * @brief  Inc and display reboot times.
  * @param  argument: Not used 
  * @retval None
  */
void reboot_times_history_info(void)
{
	uint32_t reboot_times = 0;
    
	get_key_value( "reboot_times", UINT32, (uint8_t *)(&reboot_times) );
    
	ptempstr = tempstr;
	sprintf(ptempstr," reboot_times = %d\r\n", reboot_times);
	ptemp = (uint8_t*)ptempstr;
	HAL_UART_Transmit(&huart1, ptemp, strlen(ptempstr), 0x3ff);

	reboot_times ++;
    
	set_key_value( "reboot_times", UINT32, (uint8_t *)(&reboot_times) );
}


/**
  * @brief  Uart receiver callback for data processing
  * @param  argument: UartHandle
  * @retval None
  */
void HAL_UART_RxCpltCallback(UART_HandleTypeDef *UartHandle)
{
	if(UartHandle->Instance == USART1)//user interface
	{
		//display back
		/*		*/
		if(ptempdata!=tempdata)
		{
			if(*(ptempdata-1)=='\r' || *(ptempdata-1)=='\n')
			{
				HAL_UART_Transmit_IT(&huart1, (uint8_t *)&"\r\n",2);
			}
			else
			{
				HAL_UART_Transmit_IT(&huart1,ptempdata-1,1);
				if(*(ptempdata-1)=='\b' && ptempdata>tempdata)
				{
					ptempdata-=2;
				}
			}
		}
		
		//parse command that user typein
		if(ptempdata!=tempdata && (*(ptempdata-1)=='\r' || *(ptempdata-1)=='\n'))
		{
			*(ptempdata-1)=' ';
			
			ptsub = strtok( (char*)tempdata, " " ); 
			for(int i = 0;ptsub!=NULL && i<4;i++)
			{
				//printf("%s\n",ptsub);
				char* ptas = argstack[i];
				strcpy(ptas,ptsub);
				ptsub=strtok(NULL," ");
			}
			
			//excute command
			if(strcmp(argstack[0],"getflowcount")==0) //get flowmeter1 count
			{
				if(strcmp(argstack[1],"1")==0)
				{
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter1 count = %d\r\n",flowcount1);
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"2")==0)
				{
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter2 count = %d\r\n",flowcount2);
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"3")==0)
				{
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter3 count = %d\r\n",flowcount3);
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"4")==0)
				{
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter4 count = %d\r\n",flowcount4);
					displayflag = 1;
				}
				else
				{
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter id error: %s\r\n",argstack[1]);
					displayflag = 1;
				}
			}
			else if(strcmp(argstack[0],"settolerance")==0) //set flowmeter tolerance
			{
				if(strcmp(argstack[1],"1")==0)
				{
					int tt=0;
					sscanf(argstack[2],"%d",&tt);
					setTolerance1(tt);
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter1 tolerance = %d\r\n",tt);
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"2")==0)
				{
					int tt=0;
					sscanf(argstack[2],"%d",&tt);
					setTolerance2(tt);
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter2 tolerance = %d\r\n",tt);
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"3")==0)
				{
					int tt=0;
					sscanf(argstack[2],"%d",&tt);
					setTolerance3(tt);
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter3 tolerance = %d\r\n",tt);
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"4")==0)
				{
					int tt=0;
					sscanf(argstack[2],"%d",&tt);
					setTolerance4(tt);
					ptempstr = tempstr;
					sprintf(ptempstr," flowmeter4 tolerance = %d\r\n",tt);
					displayflag = 1;
				}
				else
				{
					ptempstr = tempstr;
					sprintf(ptempstr," args error: %s\r\n",argstack[1]);
					displayflag = 1;
				}
			}
			else if(strcmp(argstack[0],"refresh")==0) //refresh sensors
			{
				if(strcmp(argstack[1],"mpu9250")==0)
				{
					refreshmpu9250();
					ptempstr = tempstr;
					sprintf(ptempstr," mpu9250 refreshed.\r\n");
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"bmp280")==0)
				{
					refreshbmp280();
					ptempstr = tempstr;
					sprintf(ptempstr," bmp280 refreshed.\r\n");
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"dht11")==0)
				{
					refreshdht11();
					ptempstr = tempstr;
					sprintf(ptempstr," dht11 refreshed.\r\n");
					displayflag = 1;
				}
				else if(strcmp(argstack[1],"vl53l1x")==0)
				{
					refreshvl53l1x();
					ptempstr = tempstr;
					sprintf(ptempstr," vl53l1x refreshed.\r\n");
					displayflag = 1;
				}
			}
			else if(strcmp(argstack[0],"getdistance")==0) //get distance
			{
					ptempstr = tempstr;
					sprintf(ptempstr," distance: %d\r\n",distance);
					displayflag = 1;
			}
			else if(strcmp(argstack[0],"getbaro")==0) //get baro
			{
					ptempstr = tempstr;
					sprintf(ptempstr," baro: %d\r\n",baro);
					displayflag = 1;
			}
			else if(strcmp(argstack[0],"getelevation")==0) //get elevation
			{
					ptempstr = tempstr;
					sprintf(ptempstr," elevation: %d\r\n",elevation);
					displayflag = 1;
			}
			else if(strcmp(argstack[0],"gettemperature")==0) //get temperature
			{
					ptempstr = tempstr;
					sprintf(ptempstr," temperature: %d\r\n",temperature);
					displayflag = 1;
			}
			else if(strcmp(argstack[0],"gethumidity")==0) //get humidity
			{
					ptempstr = tempstr;
					sprintf(ptempstr," humidity: %d\r\n",humidity);
					displayflag = 1;
			}
			else if(strcmp(argstack[0],"getposture")==0) //get posture
			{
					ptempstr = tempstr;
					sprintf(ptempstr," posture pitch:%.1f roll:%.1f  yaw:%.1f   \r\n",Pitch,Roll,Yaw);
					displayflag = 1;
			}
			else if(strcmp(argstack[0],"testmotor")==0) //get posture
			{
					motor_initpin(1,0,77);
					motor_initpin(1,1,78);
					motor_initpin(1,2,79);
					motor_initpin(1,3,80);
					motor42_fround(1,5);
			}
			else if(strcmp(argstack[0],"testmotor2")==0) //get posture
			{
					motor_initpin(1,0,77);
					motor_initpin(1,1,78);
					motor_initpin(1,2,79);
					motor_initpin(1,3,80);
					motor42_rround(1,5);
			}
			else if(strcmp(argstack[0],"lowpb4")==0) //get posture
			{
					setiolow(90);
			}
			else if(strcmp(argstack[0],"discoverotherboard")==0) //get posture
			{
					discoverflag = 1;
			}
			else if(strcmp(argstack[0],"clearotherboarddata")==0) //delete all other board sensors data
			{
				for(int j=0;j<16;j++)
				{
					for(int k=0;k<256;k++)
					{
						otherboarddata[j][k]=0;
					}
				}
					ptempstr = tempstr;
					sprintf(ptempstr," clear over.\r\n");
					displayflag = 1;
			}
			else if(strcmp(argstack[0],"getotherboarddata")==0) //get other board sensors data
			{
				if(otherboarddata[0][254]==0)
				{
					ptempstr = tempstr;
					sprintf(ptempstr," no data\r\n");
					displayflag = 1;
				}
				else
				{
					ptempstr = tempstr;
					sprintf(ptempstr," modbusaddr,baro,elevation,temperature,humidity,pwm1-4,pitch,roll,yaw,lastcoil,state\r\n");
					ptempstr = tempstr + strlen(tempstr);
				
				  for(int n=0;n<16;n++)
					{
						if(otherboarddata[n][254]==0)break;
						
						sprintf(ptempstr," ");
						ptempstr = tempstr + strlen(tempstr);
						
							sprintf(ptempstr,"0x%2x ",(int)otherboarddata[n][254]);
							ptempstr = tempstr + strlen(tempstr);
						
						for(int m=1;m<SENSORMAX;m++)
						{
							if(m==RPITCH||m==RROLL||m==RYAW)
							{
								sprintf(ptempstr,"%.1f ",(float)otherboarddata[n][m]);
							}
							else
							{
								sprintf(ptempstr,"%d ",(int)otherboarddata[n][m]);
							}
							
							ptempstr = tempstr + strlen(tempstr);
						}
						
						for(int m=CLASTPINNUM;m<254;m++)
						{
							sprintf(ptempstr,"%x ",(int)otherboarddata[n][m]);
							ptempstr = tempstr + strlen(tempstr);
						}
						
						sprintf(ptempstr,"\r\n");
						ptempstr = tempstr + strlen(tempstr);
					}
					
					ptempstr = tempstr;
					displayflag = 1;
				}
			}
			else if(strcmp(argstack[0],"reboot")==0) //reboot board
			{
				__set_FAULTMASK(1);
				NVIC_SystemReset();
			}
			else if(strcmp(argstack[0],"leavefactory")==0) //save board kvcommkey to flash to enable some board functions
			{
				set_key_value("kvcommkey", UINT32, (uint8_t *)(&commkey));
			}		
			else if(strcmp(argstack[0],"markfake")==0) //erase kvcommkey to disable some board functions
			{
				set_key_value("kvcommkey", UINT32, (uint8_t *)(&skipflag));
			}	
			else if(strcmp(argstack[0],"flownotify")==0) //set flowmeter notification on
			{
				if(strcmp(argstack[1],"on")==0)
				{
					flowmeterntf=1;
				}
				else if(strcmp(argstack[1],"off")==0)
				{
					flowmeterntf=0;
				}
				else
				{
					ptempstr = tempstr;
					sprintf(ptempstr," status error: %s\r\n",argstack[1]);
					displayflag = 1;
				}
			}			
			else if(strcmp(argstack[0],"sensornotify")==0) //set barometer and hygrometer notification on
			{
				if(strcmp(argstack[1],"on")==0)
				{
					bmpdhtntf=1;
				}
				else if(strcmp(argstack[1],"off")==0)
				{
					bmpdhtntf=0;
				}
				else
				{
					ptempstr = tempstr;
					sprintf(ptempstr," status error: %s\r\n",argstack[1]);
					displayflag = 1;
				}
			}
			else if(strcmp(argstack[0],"setmodbusaddr")==0) //set barometer and hygrometer notification off
			{
				sscanf( argstack[1], "%x", &tempaddr );
				if(tempaddr>0x00&&tempaddr<0xff)
				{
					set_key_value("modbusaddr", UINT32, (uint8_t *)(&tempaddr));
					get_key_value( "modbusaddr", UINT32, (uint8_t *)(&modbusaddr) );
					mbaddr = modbusaddr;
				}
				else
				{
					ptempstr = tempstr;
					sprintf(ptempstr," modbus address out of bound \"%s\"\r\n",argstack[1]);
					displayflag = 1;
				}
				
			}
			else if(strcmp(argstack[0],"help")==0) //manual url
			{
				ptempstr = tempstr;
				sprintf(ptempstr," manual visit https://demotree.vip\r\n");
				displayflag = 1;
			}
			else if(strlen(argstack[0])>0) //unknown command
			{
				ptempstr = tempstr;
				sprintf(ptempstr," unknown command \"%s\"\r\n contact demotree.vip for help\r\n",argstack[0]);
				displayflag = 1;
			}
			
			ptemp=(uint8_t*)argstack[0];
			memset(ptemp,0,64);
			ptemp=(uint8_t*)argstack[1];
			memset(ptemp,0,64);
			ptemp=(uint8_t*)argstack[2];
			memset(ptemp,0,64);
			ptemp=(uint8_t*)argstack[3];
			memset(ptemp,0,64);
			
			ptempdata=tempdata;
			memset(ptempdata,0,128);
		}
		
		//enable interrupt again and forward pointer
		if(HAL_UART_Receive_IT(&huart1,ptempdata,1) != HAL_OK)
		{
			HAL_UART_Transmit(&huart1, (uint8_t *)&" uart1 receiver setup error.\r\n",30,0x3ff);
			ptempdata=tempdata;		
		}
		else
		{
			ptempdata++;
			if(ptempdata>=tempdata+127)ptempdata=tempdata;
		}
	}
	if(UartHandle->Instance == USART3)//intel board communication
	{
		//while( xSemaphoreTakeFromISR( uart3sph, NULL ) != pdTRUE ) delay_ms(10);

		//enable interrupt again and forward pointer

			if(HAL_UART_Receive_IT(&huart3,pmbdata,1) != HAL_OK)
			{   
				do
				{
					uart3errtick++;
					if(uart3errtick>16384)
					{
						printf(" restart busy uart3\r\n");
						huart3.gState = HAL_UART_STATE_RESET;
						MX_USART3_UART_Init();
						pmbdata=mbdata;
						memset(pmbdata,0,256);
						pmbdata=mbdata;
						uart3errtick = 0;
					}
				}
				while(HAL_UART_Receive_IT(&huart3,pmbdata,1)!=HAL_OK);                      
			}

				pmbdata++;
				mbtick++;
				if(pmbdata>=mbdata+255)pmbdata=mbdata;
				
				uart3errtick = 0;
				mblaststamp = get_cyccnt();
				
		//xSemaphoreGiveFromISR( uart3sph , NULL);
	}
	if(UartHandle->Instance == USART2)//485 modbus
	{
		//while( xSemaphoreTakeFromISR( modbusdatasph, NULL ) != pdTRUE ) delay_us(10);
			if(HAL_UART_Receive_IT(&huart2,pmodbusdata,1) != HAL_OK)
			{   
				do
				{
					uart2errtick++;
					if(uart2errtick>16384)
					{
						printf(" restart busy uart2\r\n");
						huart2.gState = HAL_UART_STATE_RESET;
						MX_USART2_UART_Init();
						pmodbusdata=modbusdata;
						memset(pmodbusdata,0,256);
						pmodbusdata=modbusdata;
						uart2errtick = 0;
					}
				}
				while(HAL_UART_Receive_IT(&huart2,pmodbusdata,1)!=HAL_OK);                      
			}

				pmodbusdata++;
				modbustick++;
				if(pmodbusdata>=modbusdata+255)pmodbusdata=modbusdata;
				
				uart2errtick = 0;
				modbuslaststamp = get_cyccnt();
			
		//xSemaphoreGiveFromISR( modbusdatasph , NULL);	
	}
}

/* USER CODE END 4 */

/* USER CODE BEGIN Header_StartDefaultTask */
/**
  * @brief  Function implementing the defaultTask thread.
  * @param  argument: Not used 
  * @retval None
  */
/* USER CODE END Header_StartDefaultTask */
void StartDefaultTask(void const * argument)
{
  /* USER CODE BEGIN 5 */
		printf("\r\n");
		osDelay(500);
		printf("/-------------------------------------\\\r\n");
		printf("|     Welcome to Thylacine Terminal   |\r\n");
		printf("|       by alexparkmz@gmail.com       |\r\n");
		printf("|         More info plz visit         |\r\n");
		printf("|       3mn.net or demotree.vip       |\r\n");
		printf("\\-------------------------------------/\r\n");
	
	/*
	//mount tfcard
		retSD = f_mount(&fs, (TCHAR const*)SDPath, 1);
		printf("\r\n");
		printf(" starting exfat fs...\r\n");
    if(retSD)
    {
			printf(" mount error : %d \r\n",retSD);
			Error_Handler();
    }
    else
		{
			printf(" mount tfcard sucess. \r\n");
		}

		//retSD = f_mkfs((TCHAR const*)SDPath, FM_ANY,512, jpeg_buf, 16*1024);
	*/
	
		//Test IO
		//HAL_GPIO_WritePin(GPIOC, GPIO_PIN_5,GPIO_PIN_RESET);//CRL1

	defaulttaskover=1;

	/* Infinite loop */
  for(;1;)
  {
		taskYIELD();
		osDelay(500);
		
		//Test Toggle IO
		//HAL_GPIO_TogglePin(GPIOA, GPIO_PIN_1);
		
  }
  /* USER CODE END 5 */ 
}

/**
  * @brief  Period elapsed callback in non blocking mode
  * @note   This function is called  when TIM6 interrupt took place, inside
  * HAL_TIM_IRQHandler(). It makes a direct call to HAL_IncTick() to increment
  * a global variable "uwTick" used as application time base.
  * @param  htim : TIM handle
  * @retval None
  */
void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim)
{
  /* USER CODE BEGIN Callback 0 */

  /* USER CODE END Callback 0 */
  if (htim->Instance == TIM6) {
    HAL_IncTick();
  }
  /* USER CODE BEGIN Callback 1 */

  /* USER CODE END Callback 1 */
}

/**
  * @brief  This function is executed in case of error occurrence.
  * @retval None
  */
void Error_Handler(void)
{
  /* USER CODE BEGIN Error_Handler_Debug */
  /* User can add his own implementation to report the HAL error return state */

  /* USER CODE END Error_Handler_Debug */
}

#ifdef  USE_FULL_ASSERT
/**
  * @brief  Reports the name of the source file and the source line number
  *         where the assert_param error has occurred.
  * @param  file: pointer to the source file name
  * @param  line: assert_param error line source number
  * @retval None
  */
void assert_failed(uint8_t *file, uint32_t line)
{ 
  /* USER CODE BEGIN 6 */
  /* User can add his own implementation to report the file name and line number,
     tex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  /* USER CODE END 6 */
}
#endif /* USE_FULL_ASSERT */

/************************ (C) COPYRIGHT STMicroelectronics *****END OF FILE****/
