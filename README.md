# AutoMappingTimer
This is a Console App in .Net project that times partial auto mapping with individual member ignored to see which member is dragging mapping performance

###### How to  
Make sure you set up the services in align with your solution's architecture  
Make sure whereever you use DbFunctions in the map, you have a parallel ResolveUsing   
Change the TSource in Main function to your source class  
Change the TDestination in Main function to your destination class  

###### Something to be aware of
This program is written on AutoMapper version 6.2.2.0, which only support the use of mapping options in single mapping; therefore the program used 100 individual mapping instead of AutoMapper.QueryableExtensions.ProjectTo.

Each database call takes different time, so the time spans that are output in this test program serves comparison purpose, not for absolute accuracy.  
For example, if the mapping process takes significantly less time with a member X set to ignored, we could guess that the mapping for the member is the bottle neck for the whole mapping process.  
Given each database call takes different time, you may even see that mapping with a member ignored takes longer than complete mapping.
