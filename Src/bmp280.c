#include <math.h>
#include <stdbool.h>
#include "dwt_stm32_delay.h"
#include "llgpioiic.h"
#include "bmp280.h"
//#include "usart.h"

#define BMP280_PRESSURE_OSR			(BMP280_OVERSAMP_8X)
#define BMP280_TEMPERATURE_OSR		(BMP280_OVERSAMP_16X)
#define BMP280_MODE					(BMP280_PRESSURE_OSR<<2|BMP280_TEMPERATURE_OSR<<5|BMP280_NORMAL_MODE)


typedef struct 
{
    uint16_t dig_T1;                                                                /* calibration T1 data */
    int16_t dig_T2;                                                                /* calibration T2 data */
    int16_t dig_T3;                                                                /* calibration T3 data */
    uint16_t dig_P1;                                                                /* calibration P1 data */
    int16_t dig_P2;                                                                /* calibration P2 data */
    int16_t dig_P3;                                                                /* calibration P3 data */
    int16_t dig_P4;                                                                /* calibration P4 data */
    int16_t dig_P5;                                                                /* calibration P5 data */
    int16_t dig_P6;                                                                /* calibration P6 data */
    int16_t dig_P7;                                                                /* calibration P7 data */
    int16_t dig_P8;                                                                /* calibration P8 data */
    int16_t dig_P9;                                                                /* calibration P9 data */
    int32_t t_fine;                                                                /* calibration t_fine data */
} bmp280Calib;

bmp280Calib  bmp280Cal;

static uint8_t bmp280ID=0;
static bool isInit=false;
static int32_t bmp280RawPressure=0;
static int32_t bmp280RawTemperature=0;

static void bmp280GetPressure(void);
static void presssureFilter(float* in,float* out);
static float bmp280PressureToAltitude(float* pressure/*, float* groundPressure, float* groundTemp*/);

bool bmp280Init(void)
{	
  if(isInit)return true;

	IIC_Init();
  delay_ms(20);
	
	bmp280ID=iicDevReadByte(BMP280_ADDR,BMP280_CHIP_ID);
	
	if(bmp280ID==BMP280_DEFAULT_CHIP_ID)
	{
		//printf(" bmp280 started. id:0x%X\r\n",bmp280ID);  
	}
	else
	{
		return false;
	}
    //iicDevRead(BMP280_ADDR,BMP280_TEMPERATURE_CALIB_DIG_T1_LSB_REG,24,(uint8_t *)&bmp280Cal);	
		//i2cread(BMP280_ADDR,BMP280_TEMPERATURE_CALIB_DIG_T1_LSB_REG,24,(uint8_t *)&bmp280Cal);	
	iicDevRead2(BMP280_ADDR,BMP280_TEMPERATURE_CALIB_DIG_T1_LSB_REG,24,(uint8_t *)&bmp280Cal);	
	iicDevWriteByte(BMP280_ADDR,BMP280_CTRL_MEAS_REG,BMP280_MODE);
	iicDevWriteByte(BMP280_ADDR,BMP280_CONFIG_REG,5<<2);
	
//	printf("BMP280 Calibrate Registor Are: \r\n");
//	for(i=0;i<24;i++)
//		printf("Registor %2d: 0x%X\n",i,p[i]);
	
    isInit=true;
    return true;
}

static void bmp280GetPressure(void)
{
    uint8_t data[BMP280_DATA_FRAME_SIZE];

    // read data from sensor
    //iicDevRead(BMP280_ADDR,BMP280_PRESSURE_MSB_REG,BMP280_DATA_FRAME_SIZE,data);
		//i2cread(BMP280_ADDR,BMP280_PRESSURE_MSB_REG,BMP280_DATA_FRAME_SIZE,data);
		iicDevRead2(BMP280_ADDR,BMP280_PRESSURE_MSB_REG,BMP280_DATA_FRAME_SIZE,data);
    bmp280RawPressure=(int32_t)((((uint32_t)(data[0]))<<12)|(((uint32_t)(data[1]))<<4)|((uint32_t)data[2]>>4));
    bmp280RawTemperature=(int32_t)((((uint32_t)(data[3]))<<12)|(((uint32_t)(data[4]))<<4)|((uint32_t)data[5]>>4));
}

// Returns temperature in DegC, resolution is 0.01 DegC. Output value of "5123" equals 51.23 DegC
// t_fine carries fine temperature as global value
static int32_t bmp280CompensateT(int32_t adcT)
{
    int32_t var1,var2,T;

    var1=((((adcT>>3)-((int32_t)bmp280Cal.dig_T1<<1)))*((int32_t)bmp280Cal.dig_T2))>>11;
    var2=(((((adcT>>4)-((int32_t)bmp280Cal.dig_T1))*((adcT>>4)-((int32_t)bmp280Cal.dig_T1)))>>12)*((int32_t)bmp280Cal.dig_T3))>>14;
    bmp280Cal.t_fine=var1+var2;
	
    T=(bmp280Cal.t_fine*5+128)>>8;

    return T;
}

// Returns pressure in Pa as unsigned 32 bit integer in Q24.8 format (24 integer bits and 8 fractional bits).
// Output value of "24674867" represents 24674867/256 = 96386.2 Pa = 963.862 hPa
static uint32_t bmp280CompensateP(int32_t adcP)
{
    int64_t var1,var2,p;
    var1=((int64_t)bmp280Cal.t_fine)-128000;
    var2=var1*var1*(int64_t)bmp280Cal.dig_P6;
    var2=var2+((var1*(int64_t)bmp280Cal.dig_P5)<<17);
    var2=var2+(((int64_t)bmp280Cal.dig_P4)<<35);
    var1=((var1*var1*(int64_t)bmp280Cal.dig_P3)>>8)+((var1*(int64_t)bmp280Cal.dig_P2)<<12);
    var1=(((((int64_t)1)<<47)+var1))*((int64_t)bmp280Cal.dig_P1)>>33;
    if (var1==0)
        return 0;
    p=1048576-adcP;
    p=(((p<<31)-var2)*3125)/var1;
    var1=(((int64_t)bmp280Cal.dig_P9)*(p>>13)*(p>>13))>>25;
    var2=(((int64_t)bmp280Cal.dig_P8)*p)>>19;
    p=((p+var1+var2)>>8)+(((int64_t)bmp280Cal.dig_P7)<<4);
    return(uint32_t)p;
}

#define FILTER_NUM	5
#define FILTER_A	0.1f

static void presssureFilter(float* in,float* out)
{	
	*out=*in;
	/*
	static uint8_t i=0;
	static float filter_buf[FILTER_NUM]={0.0};
	double filter_sum=0.0;
	uint8_t cnt=0;	
	float deta;

	if(filter_buf[i]==0.0f)
	{
		filter_buf[i]=*in;
		*out=*in;
		if(++i>=FILTER_NUM)	
			i=0;
	} 
	else 
	{
		if(i)
			deta=*in-filter_buf[i-1];
		else 
			deta=*in-filter_buf[FILTER_NUM-1];
		
		if(fabs(deta)<FILTER_A)
		{
			filter_buf[i]=*in;
			if(++i>=FILTER_NUM)	
				i=0;
		}
		for(cnt=0;cnt<FILTER_NUM;cnt++)
		{
			filter_sum+=filter_buf[cnt];
		}
		*out=filter_sum /FILTER_NUM;
	}
	*/
}

void bmp280GetRawData(uint32_t* pressure,uint32_t* temperature)
{
	bmp280GetPressure();

	*pressure=bmp280RawPressure;
	*temperature=bmp280RawTemperature;
}

void bmp280GetData(float* pressure,float* temperature,float* asl)
{
    static float t;
    static float p;
	
	bmp280GetPressure();

	t=bmp280CompensateT(bmp280RawTemperature)/100.0;		
	p=bmp280CompensateP(bmp280RawPressure)/25600.0;		

	presssureFilter(&p,pressure);
	*temperature=(float)t;
//	*pressure=(float)p ;
	
	*asl=bmp280PressureToAltitude(pressure);
}

#define CONST_PF 0.1902630958	                                               //(1/5.25588f) Pressure factor
#define FIX_TEMP 25				                                               // Fixed Temperature. ASL is a function of pressure and temperature, but as the temperature changes so much (blow a little towards the flie and watch it drop 5 degrees) it corrupts the ASL estimates.
								                                               // TLDR: Adjusting for temp changes does more harm than good.
/*
 * Converts pressure to altitude above sea level (ASL) in meters
*/
static float bmp280PressureToAltitude(float* pressure/*, float* groundPressure, float* groundTemp*/)
{
    if (*pressure>0)
    {
        return((pow((1015.7f/ *pressure),CONST_PF)-1.0f)*(FIX_TEMP+273.15f))/0.0065f;
    }
    else
    {
        return 0;
    }
}
