<img width="150" align="right" title="cpu icon" src="./resources/cpu.png" alt_text="[Cpu icons created by Freepik - Flaticon](https://www.flaticon.com/premium-icon/cpu_707374?related_id=707576)"></img>

# Cache Simulator
<p align="justify"><b>Cache Simulator</b> was created for a <i>Computer Architecture</i> course project, as taught at the Faculty of Electrical Engineering Banja Luka. The project has been sence expanded and updated. The motivation behind this project was a better understanding of the inner working of the cache memory and its replacement algorithms.</p>

## Table of contents
- [Cache Simulator](#cache-simulator)
  - [Table of contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Cache design](#cache-design)
    - [Cache entry](#cache-entry)
  - [Replacement policy](#replacement-policy)
    - [LRU (Least-recently used)](#lru-least-recently-used)
    - [Bélády's algorithm](#béládys-algorithm)
  - [Associativity (placment policy)](#associativity-placment-policy)
    - [Direct-mapped cache](#direct-mapped-cache)
  - [Ram memory](#ram-memory)
  - [Trace file](#trace-file)
  - [Statistics (cache performance)](#statistics-cache-performance)
    - [LRU vs Bélády](#lru-vs-bélády)
  - [References](#references)
    - [Books](#books)
    - [Links](#links)
    - [Github projects](#github-projects)
  - [To-Do List](#to-do-list)

## Introduction
<p align="justify"><b>Cache Simulator</b> is a simulator implemented in C#. It supports directly mapped, N-way set associative or fully associative cache memory. It also allows LRU (Least Recently Used), Bélády's or Random replacement policy. The cache is simulated inside of the computer's RAM memory and the simulated RAM is stored on the computer's NTFS file system. The cache simulator first checks the users inputs.<br><br>
Currently there is only one cache memory level L1, or L1-D to be exact. The plan is to expand project to support both, L1-I (for instructions) and L1-D (for data), aswell as the L2 shared cache memory for CPU cores. For the simulation purposes L2 should be made slower when reading/fetching data and it should be smaller than L1.

<p><img src="./resources/single-cache.jpg" title="single cache from Operating Systems: Internals and Design Principles by William Stallings" align="center">

<p align="justify">For mapping purposes, this memory is considered to consist of a number of fixed-length blocks of K words each. That is, there are M = 2n/K blocks. Cache consists of C slots (also referred to as lines) of K words each, and the number of slots is considerably less than the number of main memory blocks (C << M).
<br><br>
If a word in a block of memory that is not in the cache is read, that block is transferred to one of the slots of the cache. Because there are more blocks than slots, an individual slot cannot be uniquely and permanently dedicated to a particular block. Therefore, each slot includes a tag that identifies which particular block is currently being stored.</p>

## Cache design
<p align="justify">Some of the key elements are briefly summarized here. Some of the important elements of cache are:
<ul>
  <li>cache size</li>
  <li>block size</li>
  <li>replacement algorithm</li>
  <li>write policy</li>
</ul>
Block size is the unit of data exchanged between cache and main memory. As the block size increases from very small to larger sizes, the hit ratio will at first
increase because of the principle of locality: the high probability that data in the
vicinity of a referenced word are likely to be referenced in the near future. As the block size increases, more useful data are brought into the cache. The hit ratio will
begin to decrease, however, as the block becomes even bigger and the probability of
using the newly fetched data becomes less than the probability of reusing the data that have to be moved out of the cache to make room for the new block.
<br><br>
If the contents of a block in the cache are altered, then it is necessary to write it back to main memory before replacing it. The write policy dictates when the memory write operation takes place. At one extreme, the writing can occur every time that the block is updated. At the other extreme, the writing occurs only when the block is replaced. The latter policy minimizes memory write operations but leaves main memory in an obsolete state.</p>

### Cache entry
<p align="justify">Cache row entries usually have the following structure:</p>
 <table align="center">
  <tr>
    <td>Tag</td>
    <td>Data Block</td>
    <td>Flag Bits</td>
  </tr>
</table>
<p align="justify">The data block (cache line/block) contains the actual data fetched from the main memory. The tag contains (part of) the address of the actual data fetched from the main memory. The "size" of the cache is the amount of main memory data it can hold. This size can be calculated as the number of bytes stored in each data block times the number of blocks stored in the cache : 
<code>size(data_block) x count(cache_lines)</code> .<br>

</p>

## Replacement policy
<p align="justify">To make room for the new entry on a cache miss, the cache may have to evict one of the existing entries. Cache algorithms are algorithms that simulator uses to manage a cache of information. When the cache is full, the algorithm must choose which items to discard to make room for the new ones. The fundamental problem with any replacement policy is that it must predict which existing cache entry is least likely to be used in the future. Predicting the future is difficult, so there is no perfect method to choose among the variety of replacement policies available.</p>

### LRU (Least-recently used)
<p align="justify">Algorithm requeres keeping track of what was used when. This was accomplished by using an <i>age bit</i> for cache-lines and then track the LRU cache-line based on the age-bits.</p>

```C#
public int Age { get; set; } = 0;
```
<p align="justify">In this implementation, every time a cache-line is used, the age of all other cache-lines in the set changes.</p>

```C#
for (var i = limit; i < limit + Associativity; ++i)
{
    ++CacheEntries[i].Age;
}
```

### Bélády's algorithm
<p align="justify">The most efficient caching algorithm would be to always discard the information that will not be needed for the longest time in the future. This optimal result is referred to as Bélády's optimal algorithm. Since it is generally impossible to predict how far in the future information will be needed, this is generally not implementable in practice. The practical minimum can be calculated only after experimentation, and one can compare the effectiveness of the actually chosen cache algorithm. For this program we have we know all of the instructions that will take place in the simulation beacause we have a finite set of instructions in stored in the trace file.</p>

## Associativity (placment policy)
<p align="justify">The placment policy decides where in the cache a copy of a particular entry of main memory will go. If the placment policy is free to choose any entry in the cache to hold the copy, the cache is <i>fully associattive</i>. At the other extreme, if each entry in main memory can go in just one place in the cache, the cache is <i>directly mapped</i>. The comprimise between the two extreems, in which each entry in main memory can go to any of N places in the cache are described as <i>N-way set associative</i>. Choosing the right value of associativity involves a trade-off. If there is eight palces to which the placment policy have mapped memory location, then to check if that location is in the cache, eight cache entries must be searched.</p>

### Direct-mapped cache
<p align="justify">It doesn't have a placment policy as such, since there is no choice of which cache entry's content to evict. This means that if two locatios map to the same entry, they continually knock each outher out.</p>

## Ram memory
<p align="justify">Ram is represented with a large binary file stored on the file system. The binary file contains randomly written data. Ram files have the following name structure <i><code>file_name-DateTime.Now:yyyyMMddHHmmss.dat</code></i>, e.q. <i>ram-20210824183840.dat</i>. Below you can find example how to create a Ram file:<br></p>

```C#
var ramSize = 5_000_000;
var ram = new RamGenerator.RamGenerator(ramSize);
ram.GenerateRam();
```

## Trace file
<p align="justify">Trace file is the name of the text file which contains memory access traces. Each line contains the following data:
<table align="center">
  <tr>
    <td>instruction_type</td>
    <td>address</td>
    <td>size_of_data</td>
    <td>data</td>
  </tr>
</table>
The instruction type can be L (load) for when data is loaded or M (modify) when data is loaded and stored. Trace file is created similray as the Ram file and the also have the same name structure, <i><code>file_name-DateTime.Now:yyyyMMddHHmmss.dat</code></i>, e.q. <i>instructions-20210824203302</i>. Below you can find example how to create a trace file:<br></p>

```C#
var numberOfInstructions = 1_000;
var trace = new TraceGenerator.TraceGenerator(numberOfInstructions);
trace.GenerateTraceFile(ramSize, cacheBlockize)
```

## Statistics (cache performance)

### LRU vs Bélády
<p align="justify">After creating the program I did a small analysis of the two algorithms and their performance. You can read the whole analysis in the <a href="./resources/algo_analysis.pdf">pdf file</a>.</p> 

## References
### Books
<ul>
  <li><p align="justify"><a href="https://www.amazon.com/Operating-Systems-Internals-Principles-International/dp/9332518807">William Stalling - <i>Operating Systems: Internals and Design Principles</i></p></a></li>
</ul>

### Links

### Github projects
Some of the projects that helped me create my project.

## To-Do List
- [ ] Add L1-I cache memory.
  - [ ] Add type '<b>I</b>' instruction to trace file.
- [ ] Add L2 cache memory (shared cache for all cores).
- [ ] Implement FIFO replacement policy.
- [ ] Implement LIFO replacement policy.
- [ ] Implement TLRU replacement policy.
- [ ] Implement LFU replacement policy.
- [ ] Implement LFUDA replacement policy.