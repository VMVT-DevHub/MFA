import React, { useState } from 'react';
import styles from './Shared.module.css';
import exclamationMark from '../exclamationmark.png';

function Login({ onSubmit, isError }) {

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    return(
        <form className={styles["shared-container"]} onSubmit={async e => {
            e.preventDefault();
            await onSubmit(username, password);
        }}>
            <p className={styles["shared-paragraph"]}>Prisijunkite su savo kompiuterio slaptažodžiu</p>
            <input 
                className={styles["shared-input"]} 
                type="text" 
                placeholder="Prisijungimo vardas" 
                value={username} 
                onChange={e => setUsername(e.target.value)}
                required 
            />
            <input 
                className={styles["shared-input"]} 
                type="password" 
                placeholder="Slaptažodis"
                value={password} 
                onChange={e => setPassword(e.target.value)} 
                required 
            /> 
            {isError && (
            <div className={styles["shared-error-container"]}>
              <img 
                src={exclamationMark} 
                alt="Error" 
                className={styles["shared-error-icon"]} />
              <p className={styles["shared-error-text"]} >Neteisingi prisijungimo duomenys</p>
            </div>
          )}
            <button 
                className={styles["shared-btn"]} 
                type="submit" 
            >
                Prisijungti
            </button>
        </form>
    );
}

export {Login}
