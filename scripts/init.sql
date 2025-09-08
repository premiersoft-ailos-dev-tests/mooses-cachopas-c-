-- ============================
-- Tabela: contacorrente
-- ============================
CREATE TABLE contacorrente (
    numero INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    ativo TINYINT(1) NOT NULL,
    senha VARCHAR(100) NOT NULL,
    salt VARCHAR(100) NOT NULL,
    cpf VARCHAR(15) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ============================
-- Tabela: idempotencia
-- ============================
CREATE TABLE idempotencia (
    chave_idempotencia CHAR(37) PRIMARY KEY,
    requisicao VARCHAR(1000) NOT NULL,
    resultado VARCHAR(1000) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ============================
-- Tabela: tarifa
-- ============================
CREATE TABLE tarifa (
    idtarifa CHAR(37) PRIMARY KEY,
    idcontacorrente CHAR(37) NOT NULL,
    datamovimento DATE NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    CONSTRAINT fk_tarifa_conta FOREIGN KEY (idcontacorrente)
        REFERENCES contacorrente(numero)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ============================
-- Tabela: transferencia
-- ============================
CREATE TABLE transferencia (
    idtransferencia BIGINT PRIMARY KEY AUTO_INCREMENT,
    datamovimento DATE NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    idcontacorrente_origem INT NOT NULL,
    idcontacorrente_destino INT NOT NULL,
    CONSTRAINT fk_transf_origem FOREIGN KEY (idcontacorrente_origem)
        REFERENCES contacorrente(numero)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_transf_destino FOREIGN KEY (idcontacorrente_destino)
        REFERENCES contacorrente(numero)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ============================
-- Tabela: movimento
-- ============================
CREATE TABLE movimento (
    idmovimento BIGINT PRIMARY KEY AUTO_INCREMENT,
    idcontacorrente INT NOT NULL,
    datamovimento DATE NOT NULL,
    tipomovimento CHAR(1) NOT NULL,
    valor DECIMAL(18,2) NOT NULL,
    CONSTRAINT fk_mov_conta FOREIGN KEY (idcontacorrente)
        REFERENCES contacorrente(numero)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;