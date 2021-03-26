/**
  * Board Designer				3mn.net demotree.vip  
	* All code between comment pairs USER CODE BEGIN and USER CODE END 
	* are written by Board Designer.
	* 
	* Copyright (c) 2020 Beijing Lizishu Tech Co.,Ltd.
  * All rights reserved.
  */
	
	
#include "modbus.h"
#include "dwt_stm32_delay.h"

uint8_t modbuff[256];
uint8_t modbufflock = 0;

void mod_send_data_old(UART_HandleTypeDef *huart,uint32_t addr,uint32_t reg,uint32_t data)
{
	uint8_t tbuff[12];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = addr;
	pir = (uint32_t*)&tbuff[4];
	*pir = reg;
	pir = (uint32_t*)&tbuff[8];
	*pir = data;
	mod_send(huart,modbusaddr,REGREAD,tbuff,12);
}

void mod_send_coilstate(UART_HandleTypeDef *huart,uint32_t pinnum,uint32_t pinstate)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = pinnum;
	pir = (uint32_t*)&tbuff[4];
	*pir = pinstate;
	mod_send(huart,modbusaddr,COILREAD,tbuff,8);
}

void mod_send_othercoilstate(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t pinnum,uint32_t pinstate)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = pinnum;
	pir = (uint32_t*)&tbuff[4];
	*pir = pinstate;
	mod_send(huart,modaddr,COILREAD,tbuff,8);
}

void mod_send_readothercoil(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t pinnum)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = pinnum;
	pir = (uint32_t*)&tbuff[4];
	*pir = 0x55aa55aa;
	mod_send(huart,modaddr,COILREAD,tbuff,8);
}

void mod_send_readotherreg(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t regaddr)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = regaddr;
	pir = (uint32_t*)&tbuff[4];
	*pir = 0x55aa55aa;
	mod_send(huart,modaddr,REGREAD,tbuff,8);
}

void mod_send_writeothercoil(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t pinnum,uint32_t pinstate)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = pinnum;
	pir = (uint32_t*)&tbuff[4];
	*pir = pinstate;
	mod_send(huart,modaddr,COILWRITE,tbuff,8);
}

void mod_send_getotherreg(UART_HandleTypeDef *huart,uint32_t modaddr,uint32_t regaddr,uint32_t sensordata)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	uint32_t* fir;
	pir = (uint32_t*)&tbuff[0];
	*pir = regaddr;
	fir = (uint32_t*)&tbuff[4];
	*fir = sensordata;
	mod_send(huart,modaddr,REGREAD,tbuff,8);
}

void mod_send_baro(UART_HandleTypeDef *huart,uint32_t baro)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RBARO;
	pir = (uint32_t*)&tbuff[4];
	*pir = baro;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_elevation(UART_HandleTypeDef *huart,uint32_t elevation)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RELEVATION;
	pir = (uint32_t*)&tbuff[4];
	*pir = elevation;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_temperature(UART_HandleTypeDef *huart,uint32_t temperature)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RTEMPERATURE;
	pir = (uint32_t*)&tbuff[4];
	*pir = temperature;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_humidity(UART_HandleTypeDef *huart,uint32_t humidity)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RHUMIDITY;
	pir = (uint32_t*)&tbuff[4];
	*pir = humidity;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_pwm1(UART_HandleTypeDef *huart,uint32_t pwm1)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RPWM1;
	pir = (uint32_t*)&tbuff[4];
	*pir = pwm1;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_pwm2(UART_HandleTypeDef *huart,uint32_t pwm2)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RPWM2;
	pir = (uint32_t*)&tbuff[4];
	*pir = pwm2;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_pwm3(UART_HandleTypeDef *huart,uint32_t pwm3)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RPWM3;
	pir = (uint32_t*)&tbuff[4];
	*pir = pwm3;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_pwm4(UART_HandleTypeDef *huart,uint32_t pwm4)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RPWM4;
	pir = (uint32_t*)&tbuff[4];
	*pir = pwm4;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_pitch(UART_HandleTypeDef *huart,float pitch)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	float* fir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RPITCH;
	fir = (float*)&tbuff[4];
	*fir = pitch;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_roll(UART_HandleTypeDef *huart,float roll)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	float* fir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RROLL;
	fir = (float*)&tbuff[4];
	*fir = roll;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_send_yaw(UART_HandleTypeDef *huart,float yaw)
{
	uint8_t tbuff[8];
	uint32_t* pir;
	float* fir;
	pir = (uint32_t*)&tbuff[0];
	*pir = RYAW;
	fir = (float*)&tbuff[4];
	*fir = yaw;
	mod_send(huart,modbusaddr,REGREAD,tbuff,8);
}

void mod_set_wj200frequency(UART_HandleTypeDef *huart,uint16_t frequency)
{
	uint8_t tbuff[4];
	tbuff[0]=0x00;
	tbuff[1]=0x01;
	tbuff[2]=frequency>>8;
	tbuff[3]=frequency&0x00ff;
	mod_send(huart,HITACHI_WJ200,REGWRITE,tbuff,4);
}

void mod_set_wj200run(UART_HandleTypeDef *huart)
{
	uint8_t tbuff[4];
	tbuff[0]=0x00;
	tbuff[1]=0x00;
	tbuff[2]=0xff;
	tbuff[3]=0x00;
	mod_send(huart,HITACHI_WJ200,COILWRITE,tbuff,4);
}

void mod_set_wj200stop(UART_HandleTypeDef *huart)
{
	uint8_t tbuff[4];
	tbuff[0]=0x00;
	tbuff[1]=0x00;
	tbuff[2]=0x00;
	tbuff[3]=0x00;
	mod_send(huart,HITACHI_WJ200,COILWRITE,tbuff,4);
}

void mod_set_wj200positiverot(UART_HandleTypeDef *huart)
{
	uint8_t tbuff[4];
	tbuff[0]=0x00;
	tbuff[1]=0x01;
	tbuff[2]=0x00;
	tbuff[3]=0x00;
	mod_send(huart,HITACHI_WJ200,COILWRITE,tbuff,4);
}

void mod_set_wj200reverserot(UART_HandleTypeDef *huart)
{
	uint8_t tbuff[4];
	tbuff[0]=0x00;
	tbuff[1]=0x01;
	tbuff[2]=0xff;
	tbuff[3]=0x00;
	mod_send(huart,HITACHI_WJ200,COILWRITE,tbuff,4);
}
	  
void mod_send(UART_HandleTypeDef *huart,uint8_t addr,uint8_t func,uint8_t* data,uint8_t datalength)
{
	//if(modbufflock!=0)return;
	//modbufflock=1;
	
	modbuff[0] = addr;
	modbuff[1] = func;
	int i=2;
	int j=0;
	for(i=2,j=0;j<datalength;i++,j++)
	{
		modbuff[i]=data[j];
	}
	uint8_t *ptr = &modbuff[0];
	uint16_t *pcrc = (uint16_t *)&modbuff[i];
	*pcrc = crc16(ptr,i);
	
	ptr = &modbuff[0];
	HAL_UART_Transmit(huart, ptr, datalength+4, 0x3ff);
	
	//modbufflock=0;
}

void mod_recv(uint8_t* buff,uint8_t recvlength,uint8_t* addr,uint8_t* func,uint8_t** data)
{
	//if(modbufflock!=0)return;
	//modbufflock=1;
	
	//int i=0;
	//for(i=0;i<recvlength;i++)
	//{
	//	modbuff[i]=buff[i];
	//}
	
	if(recvlength<4)
	{
		*addr=0;
		*func=0;
		(*data)[0]=0;
		return;
	}
	
	uint8_t *ptr = buff;
	uint16_t rc = crc16(ptr,recvlength-2);
	uint16_t *pcrc = (uint16_t *)&buff[recvlength-2];
	if(rc!=*pcrc)
	{
		//data loop adjust first
		uint8_t ft = buff[recvlength-1];
		for(int k=recvlength-1;k>0;k--)
		{
			buff[k]=buff[k-1];
		}
		buff[0]=ft;
		
		ptr = buff;
		rc = crc16(ptr,recvlength-2);
		pcrc = (uint16_t *)&buff[recvlength-2];
		
		if(rc!=*pcrc)
		{
			*addr=0;
			*func=0;
			(*data)[0]=0;
			return;
		}
	}
	
	*addr = buff[0];
	*func = buff[1];
	int j=0;
	for(j=0;j<recvlength-4;j++)
	{
		(*data)[j]=buff[j+2];
	}

	//modbufflock=0;
}

uint32_t get_gapcyccnt(void)
{
	return MOD_SILENT_US*(HAL_RCC_GetHCLKFreq() / 1000000);
}

uint16_t crc16(uint8_t *ptr,uint8_t len)
{
	unsigned long wcrc=0xffff;
	int i=0;
	int j=0;
	for(i=0;i<len;i++)
	{
		wcrc^=*ptr++;
		for(j=0;j<8;j++)
		{
			if(wcrc&0X0001)
			{
				wcrc=wcrc>>1^0xa001;
			}
			else
			{
				wcrc>>=1;
			}
		}
	}
	return wcrc;
	//return wcrc<<8|wcrc>>8;
}


//from http://mdfs.net/Info/Comp/Comms/CRC16.htm
#define POLY        0x8005
uint16_t crc16r(unsigned char *addr, int num, uint16_t crc)  
{  
	int i;  
	for (; num > 0; num--)              /* Step through bytes in memory */  
	{  
		crc = crc ^ (*addr++ << 8);     /* Fetch byte from memory, XOR into CRC top byte*/  
		for (i = 0; i < 8; i++)             /* Prepare to rotate 8 bits */  
		{  
			if (crc & 0x8000)            /* b15 is set... */  
			crc = (crc << 1) ^ POLY;    /* rotate and XOR with polynomic */  
			else                          /* b15 is clear... */  
			crc <<= 1;                  /* just rotate */  
		}                             /* Loop for 8 bits */  
		crc &= 0xFFFF;                  /* Ensure CRC remains 16-bit value */  
	}                               /* Loop until num=0 */  
	return(crc);                    /* Return updated CRC */  
}

