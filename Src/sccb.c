#include "stm32f4xx_hal.h"
#include "sccb.h"
#include "dwt_stm32_delay.h"

void SCCB_SDA_IN(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};

  __HAL_RCC_GPIOB_CLK_ENABLE();

  GPIO_InitStruct.Pin = GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
}

void SCCB_SDA_OUT(void)
{
	GPIO_InitTypeDef GPIO_InitStruct = {0};

  __HAL_RCC_GPIOB_CLK_ENABLE();

  GPIO_InitStruct.Pin = GPIO_PIN_10|GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
}
 
void SCCB_Init(void)
{
	GPIO_InitTypeDef GPIO_InitStruct = {0};

  __HAL_RCC_GPIOB_CLK_ENABLE();

  GPIO_InitStruct.Pin = GPIO_PIN_10|GPIO_PIN_11;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);
 
	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10|GPIO_PIN_11, GPIO_PIN_SET);
	SCCB_SDA_OUT();
}			 

void SCCB_Start(void)
{
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_SET);
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_SET);
    DWT_Delay_us(2);  
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_RESET);
    DWT_Delay_us(2);	 
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_RESET);
}

void SCCB_Stop(void)
{
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_RESET);
    DWT_Delay_us(2);	 
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_SET);
    DWT_Delay_us(2); 
    HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_SET);
    DWT_Delay_us(2);
}  

void SCCB_No_Ack(void)
{
	DWT_Delay_us(2);
	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_SET);	
	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_SET);	
	DWT_Delay_us(2);
	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_RESET);	
	DWT_Delay_us(2);
	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_RESET);
	DWT_Delay_us(2);
}

uint8_t SCCB_WR_Byte(uint8_t dat)
{
	uint8_t j,res;	 
	for(j=0;j<8;j++)
	{
		if(dat&0x80)HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_SET);	
		else HAL_GPIO_WritePin(GPIOB, GPIO_PIN_11, GPIO_PIN_RESET);
		dat<<=1;
		DWT_Delay_us(2);
		HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_SET);	
		DWT_Delay_us(2);
		HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_RESET);	   
	}			 
	SCCB_SDA_IN();
	DWT_Delay_us(2);
	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_SET);
	DWT_Delay_us(2);
	if(SCCB_READ_SDA)res=1;
	else res=0;
	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_RESET);		 
	SCCB_SDA_OUT();
	return res;  
}	 

uint8_t SCCB_RD_Byte(void)
{
	uint8_t temp=0,j;    
	SCCB_SDA_IN();
	for(j=8;j>0;j--)
	{		     	  
		DWT_Delay_us(2);
		HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_SET);
		temp=temp<<1;
		if(SCCB_READ_SDA)temp++;   
		DWT_Delay_us(2);
		HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_RESET);
	}	
	SCCB_SDA_OUT();
	return temp;
} 							    

uint8_t SCCB_WR_Reg(uint8_t reg,uint8_t data)
{
	uint8_t res=0;
	SCCB_Start();
	if(SCCB_WR_Byte(SCCB_ID))res=1;
	DWT_Delay_us(100);
  	if(SCCB_WR_Byte(reg))res=1;
	DWT_Delay_us(100);
  	if(SCCB_WR_Byte(data))res=1;
  	SCCB_Stop();	  
  	return	res;
}		  					    

uint8_t SCCB_RD_Reg(uint8_t reg)
{
	uint8_t val=0;
	SCCB_Start();
	SCCB_WR_Byte(SCCB_ID);
	DWT_Delay_us(100);	 
  SCCB_WR_Byte(reg);
	DWT_Delay_us(100);	  
	SCCB_Stop();   
	DWT_Delay_us(100);
	SCCB_Start();
	SCCB_WR_Byte(SCCB_ID|0X01);
	DWT_Delay_us(100);
  val=SCCB_RD_Byte();
  SCCB_No_Ack();
  SCCB_Stop();
  return val;
}
