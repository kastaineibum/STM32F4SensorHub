/*
 $License:
    Copyright (C) 2011-2012 InvenSense Corporation, All Rights Reserved.
    See https://www.invensense.com/ for License information.
 $
 */
#include "mpu9250.h"
#include "llgpioiic.h"
#include "dwt_stm32_delay.h"

extern I2C_HandleTypeDef hi2c2;

MPU9250_STATUS Init_MPU9250()
{
	uint8_t id;
	Single_Write(MPU_ADDR,MPU_PWR_MGMT1_REG,0x80);
	delay_ms(100);
	Single_Write(MPU_ADDR,MPU_PWR_MGMT1_REG, 0x00);
	MPU9250_Set_Gyro_Fsr(3);
	MPU9250_Set_Accel_Fsr(2);
	MPU9250_Set_Rate(100);
	Single_Write(MPU_ADDR,MPU_INT_EN_REG,0x00);
	Single_Write(MPU_ADDR,MPU_USER_CTRL_REG,0x00);
	Single_Write(MPU_ADDR,MPU_FIFO_EN_REG,0x00);
	Single_Write(MPU_ADDR,MPU_INTBP_CFG_REG,0x80);
	id = Single_Read(MPU_ADDR,MPU_DEVICE_ID_REG);
	#ifdef MPU9250_DUBUG
		printf(" MPU9250-ID = 0x%02x\n",id);
	#endif
	if(id == 0x71)
	{
		//printf(" MPU9250 Init Success...\n");
		Single_Write(MPU_ADDR,MPU_PWR_MGMT1_REG,0x01);
		Single_Write(MPU_ADDR,MPU_PWR_MGMT2_REG,0x00);
		MPU9250_Set_Rate(50);
		return MPU9250_OK;
	}
	else
	{
		//printf(" MPU9250 Init Fail...\n");
		return MPU9250_FAIL;
	}
}

MPU9250_STATUS READ_AKM8963_ID(uint16_t *id)
{
	Single_Write(MPU_ADDR,MPU_INTBP_CFG_REG,0x02);
	delay_ms(10);	
	Single_Write(MAG_ADDRESS,AKM8963_CNTL1_REG,0x01);
	delay_ms(10);
	*id = Single_Read(MAG_ADDRESS,AKM8963_DEVICE_ID_REG);
	#ifdef MPU9250_DUBUG
		printf(" AKM8963-ID = 0x%02x\n",*id);
	#endif	
	if(*id  == 0x48)
	{
		return  MPU9250_OK;
	}
	else
	{
		return  MPU9250_FAIL;
	}
}

uint8_t Single_Read(uint8_t SlaveAddress,uint8_t REG_Address)	 
{
  	uint8_t Data;
		HAL_I2C_Mem_Read(&hi2c2,SlaveAddress,REG_Address,1,&Data,1,100);
		return Data;
}


MPU9250_STATUS Multi_Read(uint8_t SlaveAddress,uint8_t REG_Address,uint8_t len, uint8_t *buf)
{
	if(HAL_I2C_Mem_Read(&hi2c2,SlaveAddress,REG_Address,len,buf,len,100))
	{
		return MPU9250_FAIL;
	}
	else
	{
		return MPU9250_OK;
	}
}


MPU9250_STATUS Single_Write(uint8_t SlaveAddress,uint8_t REG_Address,uint8_t REG_data)		 
{
	if(HAL_I2C_Mem_Write(&hi2c2,SlaveAddress,REG_Address,1,&REG_data,1,100))
	{
		return MPU9250_FAIL;
	}
	else
	{
		return MPU9250_OK;
	}
}


MPU9250_STATUS Multi_Write(uint8_t SlaveAddress,uint8_t REG_Address,uint8_t len,uint8_t *buf)
{
	if(HAL_I2C_Mem_Write(&hi2c2,SlaveAddress,REG_Address,len,buf,len,100))
	{
		return MPU9250_FAIL;
	}
	else
	{
		return MPU9250_OK;
	}
}

MPU9250_STATUS MPU9250_Set_Rate(uint16_t rate)
{
	uint8_t data;
	if(rate>1000)rate=1000;
	if(rate<4)rate=4;
	data = 1000/rate-1;
	data = Single_Write(MPU_ADDR,MPU_SAMPLE_RATE_REG,data);
	return MPU9250_Set_LPF(rate/2);
}

MPU9250_STATUS MPU9250_Set_LPF(uint16_t lpf)
{
	uint8_t data = 0;
	if(lpf >= 184)data=1;
	else if(lpf>=92)data=2;
	else if(lpf>=41)data=3;
	else if(lpf>=20)data=4;
	else if(lpf>=10)data=5;
	else data=6;
	return Single_Write(MPU_ADDR,MPU_CFG_REG,data);
}


MPU9250_STATUS MPU9250_Set_Gyro_Fsr(uint8_t fsr)
{
	return Single_Write(MPU_ADDR,MPU_GYRO_CFG_REG,fsr<<3);
}

MPU9250_STATUS MPU9250_Set_Accel_Fsr(uint8_t fsr)
{
	return Single_Write(MPU_ADDR,MPU_ACCEL_CFG_REG,fsr<<3);
}

MPU9250_STATUS READ_MPU9250_TEMP(float *temp)
{
	uint8_t buf[2]; 
    short raw;
	Multi_Read(MPU_ADDR,MPU_TEMP_OUTH_REG,2,buf); 
    raw=((uint16_t)buf[0]<<8)|buf[1];  
    //*temp=(36.53+((double)raw)/340)*100;
	*temp = 21 + ((double)raw)/333.87;
	return MPU9250_OK;	
}

MPU9250_STATUS READ_MPU9250_GYRO(short *gx,short *gy,short *gz)
{
	uint8_t buf[6];
	if(Multi_Read(MPU_ADDR,MPU_GYRO_XOUTH_REG,6,buf))
	{
		return MPU9250_FAIL;
	}
	else
	{
		*gx=((uint16_t)buf[0]<<8)|buf[1];  
		*gy=((uint16_t)buf[2]<<8)|buf[3];  
		*gz=((uint16_t)buf[4]<<8)|buf[5];
	}
	return MPU9250_OK;
}

MPU9250_STATUS READ_MPU9250_ACCEL(short *ax,short *ay,short *az)
{
	uint8_t buf[6];
	if(Multi_Read(MPU_ADDR,MPU_ACCEL_XOUTH_REG,6,buf))
	{
		return MPU9250_FAIL;
	}
	else
	{
		*ax=((uint16_t)buf[0]<<8)|buf[1];  
		*ay=((uint16_t)buf[2]<<8)|buf[3];  
		*az=((uint16_t)buf[4]<<8)|buf[5];
	}
	return MPU9250_OK;
}

MPU9250_STATUS READ_MPU9250_MAG(short *mx,short *my,short *mz)
{
	uint8_t buf[6];
	Single_Write(MPU_ADDR,MPU_INTBP_CFG_REG,0x02);
	delay_ms(10);	
	Single_Write(MAG_ADDRESS,AKM8963_CNTL1_REG,0x01);
	delay_ms(10);
	if(Multi_Read(MAG_ADDRESS,AKM8963_MAG_XOUTL_REG,6,buf))
	{
		return MPU9250_FAIL;
	}
	else
	{
		*mx=((uint16_t)buf[1]<<8)|buf[0];  
		*my=((uint16_t)buf[3]<<8)|buf[2];  
		*mz=((uint16_t)buf[5]<<8)|buf[4];
	}
	return MPU9250_OK;
}
