# Banco de dados - [A2-Gestamp-App]

## 1. Visão Geral

Vamos usar SQLite pois é simples e precisa somente do armazenamento local.

## 2. Dados e Referências

# Database Schema

## 1. production_shift
Turno de produção consolidado.


* **id** (PK): Inteiro, não nulo.
* **ShiftNumber**: Numero do turno.
* **startDate**: Data e hora do turno.
* **endDate**: Data e hora do turno.
* **produced**: Total produzido.
* **approved**: Total aprovado.
* **reproved**: Total reprovado.
* **rejectionRate**: Taxa de rejeição.
* **createdAt**: Data de criação.

## 2. inspection
Inspeções individuais das câmeras.

* **id** (PK): Inteiro, auto-incremento.
* **date**: Data e hora da inspeção.
* **originalJudgement**: Julgamento da câmera (obrigatório).
* **firstImagePath**: Caminho da 1ª imagem (obrigatório).
* **secondtImagePath**: Caminho da 2ª imagem (padrão: 'now').
* **thirdyImagePath**: Caminho da 3ª imagem (opcional).
* **finalJudgement**: Ajuste manual de julgamento (opcional).
* **production_id** (FK): Vincula com `production_shift.id` (opcional).