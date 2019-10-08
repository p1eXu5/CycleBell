# Cycle Bell 
*Read this in other languages:* [Русский](README.md)

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/demonstration.png" alt="CycleBell. Main window." width="250" />

Download: [CycleBell 1.0.1](https://github.com/p1eXu5/CycleBell/releases/download/1.0.1/CycleBell.msi)


### Background

The task was to create a lightweight break notification system for two categories of employees: for smokers - with 5 min break every hour, and for non-smokers - with 10 min break every two hours.
Also, one of the requirements was to make it a "turn-on-and-forget" solution, i.e. after the timer is started, it notifies on breaks at the set time round-the-clock. No suitable tool was found at that time. There existed either paid solutions, or those that were difficult to set up, or those that were not flexible enough to solve the given task. Therefore, this timer was created.

### Features

- Installs locally.
- Able to save and import presets.
- Has 5 preset sound samples, plus you can add your own.
- Runs on Windows 7, Windows 10.
- Can be used as a Pomodoro-timer.

<br/>

Interface
------

#### Start Window:

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/start-window.png" alt="CycleBell. Start Window." width="250" />

#### Меню "File":

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/menu-file.png" alt="CycleBell. Menu File" />
 
<table border="0">
  <tbody>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item1.png" alt="Item #1" align="top" />
      </td>
      <td>
        Create the new preset.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item2.png" alt="Item #2" align="top" />
      </td>
      <td>
        Append presets from a xml-file.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item3.png" alt="Item #3" align="top"  />
      </td>
      <td>
        Export presets to xml-file. If there are no presets, then menu item is disabled.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item4.png" alt="Item #4" align="top" />
      </td>
      <td>
        Clear presets. If there are no presets, then menu item is disabled.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item5.png" alt="Item #5" align="top" />
      </td>
      <td>
        Exit from program.
      </td>
    </tr>
  </tbody>
</table>

<br/>

#### Menu "Settings":

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/menu-settings.png" alt="CycleBell. Menu Settings" />
 
<table border="0">
  <tbody>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item6.png" alt="Item #6" align="top" />
      </td>
      <td>
        If not checked then start bell is off. 
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item7.png" alt="Item #7" align="top" />
      </td>
      <td>
        If checked, then timer is looped. If there are no presets, then option is disabled.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item8.png" alt="Item #8" align="top"  />
      </td>
      <td>
        Select default sound.
      </td>
    </tr>
  </tbody>
</table>

<br/>

#### Main Window:

<img src="https://github.com/p1eXu5/CycleBell/blob/development/images/main-window.png" alt="CycleBell. Main Window." />
 
<table border="0">
  <tbody>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item9.png" alt="Item #9" align="top" />
      </td>
      <td>
        Naming or selection the preset.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item10.png" alt="Item #10" align="top" />
      </td>
      <td>
        Delete preset.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item11.png" alt="Item #11" align="top"  />
      </td>
      <td>
        Setup the start time, +1 minute or +5 minutes from current time.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item12.png" alt="Item #12" align="top"  />
      </td>
      <td>
        Setup the start time.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item13.png" alt="Item #13" align="top"  />
      </td>
      <td>
        Label with the name of current time point.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item14.png" alt="Item #14" align="top"  />
      </td>
      <td>
        The name of the adding time point.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item15.png" alt="Item #15" align="top"  />
      </td>
      <td>
        Duration or completion time of the adding time point.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item16.png" alt="Item #16" align="top"  />
      </td>
      <td>
        Kind of time switcher, duration or completion time.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item17.png" alt="Item #17" align="top"  />
      </td>
      <td>
        Sound setup for the adding time point.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item18.png" alt="Item #18" align="top"  />
      </td>
      <td>
        The number of the time loop.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item19.png" alt="Item #19" align="top"  />
      </td>
      <td>
        Add the new time point into the preset.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item20.png" alt="Item #20" align="top"  />
      </td>
      <td>
        If checked, then timer is looped.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item21.png" alt="Item #21" align="top"  />
      </td>
      <td>
        Play/pause.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item22.png" alt="Item #22" align="top"  />
      </td>
      <td>
        Stop.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item23.png" alt="Item #23" align="top"  />
      </td>
      <td>
        Start the bell on left mouse button, stop the bell if to click again. On or off the start bell on right mouse button.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item24.png" alt="Item #24" align="top"  />
      </td>
      <td>
        Preset time points section.
      </td>
    </tr>
    <tr>
      <td width="50" align="center" valign="middle">
        <img src="https://github.com/p1eXu5/CycleBell/blob/development/images/item25.png" alt="Item #25" align="top"  />
      </td>
      <td>
        Number of loops of the time segment.
      </td>
    </tr>
  </tbody>
</table>

