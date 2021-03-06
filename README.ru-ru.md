# Cycle Bell 
*Прочитать на других языках:* [English](README.md)

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/demonstration.png" alt="CycleBell. Окно программы" width="250" />

Скачать: [CycleBell 1.0.1](https://github.com/p1eXu5/CycleBell/releases/download/1.0.1/CycleBell.msi)


### Предыстория

Стояла задача сделать легковесную систему оповещение о перерывах для двух категорий сотрудников, для курящих - каждый час по 5 минут, для некурящих - по 10 минут каждые два часа. Также одним из требований было "включил и забыл", т.е. запустил таймер, и он круглосуточно отзванивает перерывы в установленное время. Подходящего инструмента на тот момент найдено не было - то были либо платные решения, либо не отличающиеся простотой настройки, либо недостаточно гибкие для решения этой задачи. В результате был написан данный таймер.

### Описание возможностей

- Устанавливается локально.
- Есть возможность сохранять и импортировать пресеты.
- Предустановлены 5 сэмплов звуковых сигналов, можно добавлять свои.
- Работает под управлением операционных систем Windows 7, Windows 10.
- Возможно использовать в качестве Pomodoro-таймера.

<br/>

Описание интерфейса
------

#### Начальное окно программы:

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/start-window.png" alt="CycleBell. Начальное окно" width="250" />

#### Меню "File":

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/menu-file.png" alt="CycleBell. Меню File" />
 
<table border="0">
  <tbody>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item1.png" alt="элемента списка #1" align="top" />
      </td>
      <td>
        Создание нового пресета.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item2.png" alt="элемента списка #2" align="top" />
      </td>
      <td>
        Добавление пресетов из xml-файла.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item3.png" alt="элемента списка #3" align="top"  />
      </td>
      <td>
        Экспорт пресетов в xml-файл. Неактивна, если ни один пресет не создан.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item4.png" alt="элемента списка #4" align="top" />
      </td>
      <td>
        Очистка списка пресетов. Неактивна, если ни один пресет не создан.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item5.png" alt="элемента списка #5" align="top" />
      </td>
      <td>
        Выход из программы.
      </td>
    </tr>
  </tbody>
</table>

<br/>

#### Меню "Settings":

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/menu-settings.png" alt="CycleBell. Меню Settings" />
 
<table border="0">
  <tbody>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item6.png" alt="элемента списка #6" align="top" />
      </td>
      <td>
        Если данная опция включает первый звонок. 
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item7.png" alt="элемента списка #7" align="top" />
      </td>
      <td>
        Опция зацикливает таймер. Неактивна, если ни один пресет не создан.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item8.png" alt="элемента списка #8" align="top"  />
      </td>
      <td>
        Выбор мелодии звонка по умолчанию.
      </td>
    </tr>
  </tbody>
</table>

<br/>

#### Главное окно:

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/main-window.png" alt="CycleBell. Меню главного окна." />
 
<table border="0">
  <tbody>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item9.png" alt="элемента списка #9" align="top" />
      </td>
      <td>
        Наименование и выбор пресета.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item10.png" alt="элемента списка #10" align="top" />
      </td>
      <td>
        Удаление пресета.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item11.png" alt="элемента списка #11" align="top"  />
      </td>
      <td>
        Установка времени начала отсчёта, +1 минута, либо +5 минут от текущего времени.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item12.png" alt="элемента списка #12" align="top"  />
      </td>
      <td>
        Установка времени начала отсчета.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item13.png" alt="элемента списка #13" align="top"  />
      </td>
      <td>
        Имя текущего временной точки.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item14.png" alt="элемента списка #14" align="top"  />
      </td>
      <td>
        Имя добавляемой временной точки.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item15.png" alt="элемента списка #15" align="top"  />
      </td>
      <td>
        Длительность, либо время окончания временной точки.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item16.png" alt="элемента списка #16" align="top"  />
      </td>
      <td>
        Переключатель типа задаваемого времени временной точки, длительность, либо время окончания.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item17.png" alt="элемента списка #17" align="top"  />
      </td>
      <td>
        Установка мелодии звонка для временной точки.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item18.png" alt="элемента списка #18" align="top"  />
      </td>
      <td>
        Номер временного сегмента.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item19.png" alt="элемента списка #19" align="top"  />
      </td>
      <td>
        Кнопка добавления временной точки в пресет.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item20.png" alt="элемента списка #20" align="top"  />
      </td>
      <td>
        Кнопка зацикливания таймера.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item21.png" alt="элемента списка #21" align="top"  />
      </td>
      <td>
        Запуск/пауза.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item22.png" alt="элемента списка #22" align="top"  />
      </td>
      <td>
        Остановка таймера.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item23.png" alt="элемента списка #23" align="top"  />
      </td>
      <td>
        Нажатие левой кнопки мыши инициирует звонок, повторное нажатие - останавливает его. Правая кнопка мыши - включает или отключает первый звонок.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item24.png" alt="элемента списка #24" align="top"  />
      </td>
      <td>
        Секция временных точек пресета.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item25.png" alt="элемента списка #25" align="top"  />
      </td>
      <td>
        Количество повторений временного сегмента.
      </td>
    </tr>
  </tbody>
</table>

