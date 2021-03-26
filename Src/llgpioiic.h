#ifndef __MYIIC_H
#define __MYIIC_H
#include "dwt_stm32_delay.h"

void SDA_IN(void);
void SDA_OUT(void);
 
#define READ_SDA		HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_11)
#define IIC_SCL_0  	{HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_RESET);}
#define IIC_SDA_0  	{HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_RESET);}

#define IIC_SCL_1  	{HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_SET);}
#define IIC_SDA_1  	{HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_SET);}

#define	I2C_Transmitter   0
#define	I2C_Receiver      1	
#define true 1
#define false 0 
#define TRUE  0
#define FALSE 0xff

void IIC_Init(void);			 
void IIC_Start(void);
void IIC_Stop(void);
void IIC_Send_Byte(uint8_t txd);
uint8_t IIC_Read_Byte(unsigned char ack);
uint8_t IIC_Wait_Ack(void);
void IIC_Ack(void);
void IIC_NAck(void);

void IIC_Write_One_Byte(uint8_t daddr,uint8_t addr,uint8_t data);
uint8_t IIC_Read_One_Byte(uint8_t daddr,uint8_t addr);	 

uint8_t i2cWriteBuffer(uint8_t addr, uint8_t reg, uint8_t len, uint8_t *data);
uint8_t i2cReadBuffer(uint8_t addr, uint8_t reg, uint8_t len, uint8_t* buf);
uint8_t i2cwrite(uint8_t addr, uint8_t reg, uint8_t len, uint8_t * data);
uint8_t i2cread(uint8_t addr, uint8_t reg, uint8_t len, uint8_t *buf);

uint8_t i2cWriteBuffer2(uint8_t addr, uint8_t len, uint8_t *data);
uint8_t i2cReadBuffer2(uint8_t addr, uint8_t len, uint8_t* buf);
uint8_t i2cwrite2(uint8_t addr, uint8_t len, uint8_t * data);
uint8_t i2cread2(uint8_t addr, uint8_t len, uint8_t *buf);

uint8_t iicDevReadByte(uint8_t devaddr,uint8_t addr);
void iicDevRead(uint8_t devaddr,uint8_t addr,uint8_t len,uint8_t *rbuf);
void iicDevRead2(uint8_t addr, uint8_t reg, uint8_t len, uint8_t* buf);
void iicDevWriteByte(uint8_t devaddr,uint8_t addr,uint8_t data);
void iicDevWrite(uint8_t devaddr,uint8_t addr,uint8_t len,uint8_t *wbuf);

#endif
